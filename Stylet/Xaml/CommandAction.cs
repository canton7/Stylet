using Stylet.Logging;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Input;
using Expressions = System.Linq.Expressions;

namespace Stylet.Xaml
{
    /// <summary>
    /// ICommand returned by ActionExtension for binding buttons, etc, to methods on a ViewModel.
    /// If the method has a parameter, CommandParameter is passed
    /// </summary>
    /// <remarks>
    /// Watches the current View.ActionTarget, and looks for a method with the given name, calling it when the ICommand is called.
    /// If a bool property with name Get(methodName) exists, it will be observed and used to enable/disable the ICommand.
    /// </remarks>
    public class CommandAction : ICommand
    {
        private static readonly ILogger logger = LogManager.GetLogger(typeof(CommandAction));

        /// <summary>
        /// Gets the View to grab the View.ActionTarget from
        /// </summary>
        public DependencyObject Subject { get; private set; }

        /// <summary>
        /// Gets the method name. E.g. if someone's gone Buttom Command="{s:Action MyMethod}", this is MyMethod.
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// Generated accessor to grab the value of the guard property, or null if there is none
        /// </summary>
        private Func<bool> guardPropertyGetter;

        /// <summary>
        /// MethodInfo for the method to call. This has to exist, or we throw a wobbly
        /// </summary>
        private MethodInfo targetMethodInfo;

        private object target;

        private readonly ActionUnavailableBehaviour targetNullBehaviour;
        private readonly ActionUnavailableBehaviour actionNonExistentBehaviour;

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandAction"/> class
        /// </summary>
        /// <param name="subject">View to grab the View.ActionTarget from</param>
        /// <param name="methodName">Method name. the MyMethod in Buttom Command="{s:Action MyMethod}".</param>
        /// <param name="targetNullBehaviour">Behaviour for it the relevant View.ActionTarget is null</param>
        /// <param name="actionNonExistentBehaviour">Behaviour for if the action doesn't exist on the View.ActionTarget</param>
        public CommandAction(DependencyObject subject, string methodName, ActionUnavailableBehaviour targetNullBehaviour, ActionUnavailableBehaviour actionNonExistentBehaviour)
        {
            this.Subject = subject;
            this.MethodName = methodName;
            this.targetNullBehaviour = targetNullBehaviour;
            this.actionNonExistentBehaviour = actionNonExistentBehaviour;

            this.UpdateGuardAndMethod();

            // Observe the View.ActionTarget for changes, and re-bind the guard property and MethodInfo if it changes
            PropertyChangeNotifier.AddValueChanged(this.Subject, View.ActionTargetProperty, (o, e) => this.UpdateGuardAndMethod());
        }

        private string GuardName
        {
            get { return "Can" + this.MethodName; }
        }

        private void UpdateGuardAndMethod()
        {
            var newTarget = View.GetActionTarget(this.Subject);
            MethodInfo targetMethodInfo = null;

            // If it's being set to the initial value, ignore it
            // At this point, we're executing the View's InitializeComponent method, and the ActionTarget hasn't yet been assigned
            // If they've opted to throw if the target is null, then this will cause that exception.
            // We'll just wait until the ActionTarget is assigned, and we're called again
            if (newTarget == View.InitialActionTarget)
            {
                this.target = newTarget;
                return;
            }

            this.guardPropertyGetter = null;
            if (newTarget == null)
            {
                // If it's Enable or Disable we don't do anything - CanExecute will handle this
                if (this.targetNullBehaviour == ActionUnavailableBehaviour.Throw)
                {
                    var e = new ActionTargetNullException(String.Format("ActionTarget on element {0} is null (method name is {1})", this.Subject, this.MethodName));
                    logger.Error(e);
                    throw e;
                }
                else
                {
                    logger.Info("ActionTarget on element {0} is null (method name is {1}), but NullTarget is not Throw, so carrying on", this.Subject, this.MethodName);
                }
            }
            else
            {
                var newTargetType = newTarget.GetType();

                var guardPropertyInfo = newTargetType.GetProperty(this.GuardName);
                if (guardPropertyInfo != null)
                {
                    if (guardPropertyInfo.PropertyType == typeof(bool))
                    {
                        var targetExpression = Expressions.Expression.Constant(newTarget);
                        var propertyAccess = Expressions.Expression.Property(targetExpression, guardPropertyInfo);
                        this.guardPropertyGetter = Expressions.Expression.Lambda<Func<bool>>(propertyAccess).Compile();
                    }
                    else
                    {
                        logger.Warn("Found guard property {0} for action {1} on target {2}, but its return type wasn't bool. Therefore, ignoring", this.GuardName, this.MethodName, newTarget);
                    }
                }

                targetMethodInfo = newTargetType.GetMethod(this.MethodName);
                if (targetMethodInfo == null)
                {
                    if (this.actionNonExistentBehaviour == ActionUnavailableBehaviour.Throw)
                    {
                        var e = new ActionNotFoundException(String.Format("Unable to find method {0} on {1}", this.MethodName, newTargetType.Name));
                        logger.Error(e);
                        throw e;
                    }
                    else
                    {
                        logger.Warn("Unable to find method {0} on {1}, but ActionNotFound is not Throw, so carrying on", this.MethodName, newTargetType.Name);
                    }
                }
                else
                {
                    var methodParameters = targetMethodInfo.GetParameters();
                    if (methodParameters.Length > 1)
                    {
                        var e = new ActionSignatureInvalidException(String.Format("Method {0} on {1} must have zero or one parameters", this.MethodName, newTargetType.Name));
                        logger.Error(e);
                        throw e;
                    }
                }
            }

            var oldTarget = this.target as INotifyPropertyChanged;
            if (oldTarget != null)
                PropertyChangedEventManager.RemoveHandler(oldTarget, this.PropertyChangedHandler, this.GuardName);

            var inpc = newTarget as INotifyPropertyChanged;
            if (this.guardPropertyGetter != null && inpc != null)
                PropertyChangedEventManager.AddHandler(inpc, this.PropertyChangedHandler, this.GuardName);

            this.target = newTarget;
            this.targetMethodInfo = targetMethodInfo;

            this.UpdateCanExecute();
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            var handler = this.CanExecuteChanged;
            // So. While we're safe firing PropertyChanged events on a non-UI thread, we
            // are not safe firing CanExecuteChanged events on other threads...
            // Therefore make sure we're on the UI thread
            if (handler != null)
                Stylet.Execute.OnUIThread(() => handler(this, EventArgs.Empty));
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            // This can happen if the ActionTarget hasn't been set from its default - 
            // e.g. if the button/etc in question is in a ContextMenu/Popup, which attached properties
            // aren't inherited by.
            // Show the control as enabled, but throw if they try and click on it
            if (this.target == View.InitialActionTarget)
                return true;

            // It's enabled only if both the targetNull and actionNonExistent tests pass

            // Throw is handled when the target is set
            if (this.target == null)
                return this.targetNullBehaviour != ActionUnavailableBehaviour.Disable;

            // Throw is handled when the target is set
            if (this.targetMethodInfo == null)
            {
                if (this.actionNonExistentBehaviour == ActionUnavailableBehaviour.Disable)
                    return false;
                else
                    return true;
            }

            if (this.guardPropertyGetter == null)
                return true;

            return this.guardPropertyGetter();
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// The method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            // If we've made it this far and the target is still the default, then something's wrong
            // Make sure they know
            if (this.target == View.InitialActionTarget)
            {
                var e = new ActionNotSetException(String.Format("View.ActionTarget not on control {0} (method {1}). " +
                    "This probably means the control hasn't inherited it from a parent, e.g. because a ContextMenu or Popup sits in the visual tree. " +
                    "You will need so set 's:View.ActionTarget' explicitly. See the wiki for more details.", this.Subject, this.MethodName));
                logger.Error(e);
                throw e;
            }

            // Any throwing would have been handled prior to this
            if (this.target == null || this.targetMethodInfo == null)
                return;

            // This is not going to be called very often, so don't bother to generate a delegate, in the way that we do for the method guard
            var parameters = this.targetMethodInfo.GetParameters().Length == 1 ? new[] { parameter } : null;
            logger.Info("Invoking method {0} on target {1} with parameters ({2})", this.MethodName, this.target, parameters == null ? "none" : String.Join(", ", parameters));

            try
            {
                this.targetMethodInfo.Invoke(this.target, parameters);
            }
            catch (TargetInvocationException e)
            {
                // Be nice and unwrap this for them
                // They want a stack track for their VM method, not us
                logger.Error(e.InnerException, String.Format("Failed to invoke method {0} on target {1} with parameters ({2})", this.MethodName, this.target, parameters == null ? "none" : String.Join(", ", parameters)));
                // http://stackoverflow.com/a/17091351/1086121
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }
        }
    }
}
