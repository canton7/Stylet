using Stylet.Logging;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;

namespace Stylet.Xaml
{
    /// <summary>
    /// Created by ActionExtension, this can return a delegate suitable adding binding to an event, and can call a method on the View.ActionTarget
    /// </summary>
    public class EventAction
    {
        private static readonly ILogger logger = LogManager.GetLogger(typeof(EventAction));
        private static readonly MethodInfo invokeCommandMethodInfo = typeof(EventAction).GetMethod("InvokeCommand", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly ActionUnavailableBehaviour targetNullBehaviour;
        private readonly ActionUnavailableBehaviour actionNonExistentBehaviour;

        /// <summary>
        /// View whose View.ActionTarget we watch
        /// </summary>
        private readonly DependencyObject subject;

        /// <summary>
        /// The MyMethod in {s:Action MyMethod}, this is what we call when the event's fired
        /// </summary>
        private readonly string methodName;

        /// <summary>
        /// Type of event handler
        /// </summary>
        private readonly Type eventHandlerType;

        /// <summary>
        /// MethodInfo for the method to call. This has to exist, or we throw a wobbly
        /// </summary>
        private MethodInfo targetMethodInfo;

        private object target;

        /// <summary>
        /// Initialises a new instance of the <see cref="EventAction"/> class
        /// </summary>
        /// <param name="subject">View whose View.ActionTarget we watch</param>
        /// <param name="eventHandlerType">Type of event handler we're returning a delegate for</param>
        /// <param name="methodName">The MyMethod in {s:Action MyMethod}, this is what we call when the event's fired</param>
        /// <param name="targetNullBehaviour">Behaviour for it the relevant View.ActionTarget is null</param>
        /// <param name="actionNonExistentBehaviour">Behaviour for if the action doesn't exist on the View.ActionTarget</param>
        public EventAction(DependencyObject subject, Type eventHandlerType, string methodName, ActionUnavailableBehaviour targetNullBehaviour, ActionUnavailableBehaviour actionNonExistentBehaviour)
        {
            if (targetNullBehaviour == ActionUnavailableBehaviour.Disable)
                throw new ArgumentException("Setting NullTarget = Disable is unsupported when used on an Event");
            if (actionNonExistentBehaviour == ActionUnavailableBehaviour.Disable)
                throw new ArgumentException("Setting ActionNotFound = Disable is unsupported when used on an Event");

            this.subject = subject;
            this.eventHandlerType = eventHandlerType;
            this.methodName = methodName;
            this.targetNullBehaviour = targetNullBehaviour;
            this.actionNonExistentBehaviour = actionNonExistentBehaviour;

            this.UpdateMethod();

            // Observe the View.ActionTarget for changes, and re-bind the guard property and MethodInfo if it changes
            PropertyChangeNotifier.AddValueChanged(this.subject, View.ActionTargetProperty, (o, e) => this.UpdateMethod());
        }

        private void UpdateMethod()
        {
            var newTarget = View.GetActionTarget(this.subject);
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

            if (newTarget == null)
            {
                if (this.targetNullBehaviour == ActionUnavailableBehaviour.Throw)
                {
                    var e = new ActionTargetNullException(String.Format("ActionTarget on element {0} is null (method name is {1})", this.subject, this.methodName));
                    logger.Error(e);
                    throw e;
                }
                else
                {
                    logger.Info("ActionTarget on element {0} is null (method name is {1}), nut NullTarget is not Throw, so carrying on", this.subject, this.methodName);
                }
            }
            else
            {
                var newTargetType = newTarget.GetType();
                targetMethodInfo = newTargetType.GetMethod(this.methodName);
                if (targetMethodInfo == null)
                {
                    if (this.actionNonExistentBehaviour == ActionUnavailableBehaviour.Throw)
                    {
                        var e = new ActionNotFoundException(String.Format("Unable to find method {0} on {1}", this.methodName, newTargetType.Name));
                        logger.Error(e);
                        throw e;
                    }
                    else
                    {
                        logger.Warn("Unable to find method {0} on {1}, but ActionNotFound is not Throw, so carrying on", this.methodName, newTargetType.Name);
                    }
                }
                else
                {
                    var methodParameters = targetMethodInfo.GetParameters();
                    if (!(methodParameters.Length == 0 ||
                        (methodParameters.Length == 1 && typeof(EventArgs).IsAssignableFrom(methodParameters[0].ParameterType)) ||
                        (methodParameters.Length == 2 && typeof(EventArgs).IsAssignableFrom(methodParameters[1].ParameterType))))
                    {
                        var e = new ActionSignatureInvalidException(String.Format("Method {0} on {1} must have the signatures void Method(), void Method(EventArgsOrSubClass e), or void Method(object sender, EventArgsOrSubClass e)", this.methodName, newTargetType.Name));
                        logger.Error(e);
                        throw e;
                    }
                }
            }

            this.target = newTarget;
            this.targetMethodInfo = targetMethodInfo;
        }

        /// <summary>
        /// Return a delegate which can be added to the targetProperty
        /// </summary>
        /// <returns>An event hander, which, when invoked, will invoke the action</returns>
        public Delegate GetDelegate()
        {
            var del = Delegate.CreateDelegate(this.eventHandlerType, this, invokeCommandMethodInfo, false);
            if (del == null)
            {
                var e = new ActionEventSignatureInvalidException(String.Format("Event being bound to does not have the '(object sender, EventArgsOrSubclass e)' signature we were expecting. Method {0} on target {1}", this.methodName, this.target));
                logger.Error(e);
                throw e;
            }
            return del;
        }

        // ReSharper disable once UnusedMember.Local
        private void InvokeCommand(object sender, EventArgs e)
        {
            // If we've made it this far and the target is still the default, then something's wrong
            // Make sure they know
            if (this.target == View.InitialActionTarget)
            {
                var ex = new ActionNotSetException(String.Format("View.ActionTarget not on control {0} (method {1}). " +
                    "This probably means the control hasn't inherited it from a parent, e.g. because a ContextMenu or Popup sits in the visual tree. " +
                    "You will need so set 's:View.ActionTarget' explicitly. See the wiki for more details.", this.subject, this.methodName));
                logger.Error(ex);
                throw ex;
            }

            // Any throwing will have been handled above
            if (this.target == null || this.targetMethodInfo == null)
                return;

            object[] parameters;
            switch (this.targetMethodInfo.GetParameters().Length)
            {
                case 1:
                    parameters = new object[] { e };
                    break;
                    
                case 2:
                    parameters = new[] { sender, e };
                    break;

                default:
                    parameters = null;
                    break;
            }

            logger.Info("Invoking method {0} on target {1} with parameters ({2})", this.methodName, this.target, parameters == null ? "none" : String.Join(", ", parameters));

            try
            {
                this.targetMethodInfo.Invoke(this.target, parameters);
            }
            catch (TargetInvocationException ex)
            {
                // Be nice and unwrap this for them
                // They want a stack track for their VM method, not us
                logger.Error(ex.InnerException, String.Format("Failed to invoke method {0} on target {1} with parameters ({2})", this.methodName, this.target, parameters == null ? "none" : String.Join(", ", parameters)));
                // http://stackoverflow.com/a/17091351/1086121
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }
    }

    /// <summary>
    /// You tried to use an EventAction with an event that doesn't follow the EventHandler signature
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class ActionEventSignatureInvalidException : Exception
    {
        internal ActionEventSignatureInvalidException(string message) : base(message) { }
    }
}
