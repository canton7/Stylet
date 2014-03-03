using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Expressions = System.Linq.Expressions;

namespace Stylet
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
        /// <summary>
        /// View to grab the View.ActionTarget from
        /// </summary>
        private FrameworkElement subject;

        /// <summary>
        /// Method name. E.g. if someone's gone Buttom Command="{s:Action MyMethod}", this is MyMethod.
        /// </summary>
        private string methodName;

        /// <summary>
        /// Generated accessor to grab the value of the guard property, or null if there is none
        /// </summary>
        private Func<bool> guardPropertyGetter;

        /// <summary>
        /// MethodInfo for the method to call. This has to exist, or we throw a wobbly
        /// </summary>
        private MethodInfo targetMethodInfo;

        private object target;

        /// <summary>
        /// Create a new ActionCommand 
        /// </summary>
        /// <param name="subject">View to grab the View.ActionTarget from</param>
        /// <param name="methodName">Method name. the MyMethod in Buttom Command="{s:Action MyMethod}".</param>
        public CommandAction(FrameworkElement subject, string methodName)
        {
            this.subject = subject;
            this.methodName = methodName;

            this.UpdateGuardAndMethod();

            // Observe the View.ActionTarget for changes, and re-bind the guard property and MethodInfo if it changes
            DependencyPropertyDescriptor.FromProperty(View.ActionTargetProperty, typeof(View)).AddValueChanged(this.subject, (o, e) => this.UpdateGuardAndMethod());
        }

        private string GuardName
        {
            get { return "Can" + this.methodName; }
        }

        private void UpdateGuardAndMethod()
        {
            var newTarget = View.GetActionTarget(this.subject);
            MethodInfo targetMethodInfo = null;
            
            this.guardPropertyGetter = null;
            if (newTarget != null)
            {
                var newTargetType = newTarget.GetType();

                var guardPropertyInfo = newTargetType.GetProperty(this.GuardName);
                if (guardPropertyInfo != null && guardPropertyInfo.PropertyType == typeof(bool))
                {
                    var targetExpression = Expressions.Expression.Constant(newTarget);
                    var propertyAccess = Expressions.Expression.Property(targetExpression, guardPropertyInfo);
                    this.guardPropertyGetter = Expressions.Expression.Lambda<Func<bool>>(propertyAccess).Compile();
                }

                targetMethodInfo = newTargetType.GetMethod(this.methodName);
                if (targetMethodInfo == null)
                    throw new ArgumentException(String.Format("Unable to find method {0} on {1}", this.methodName, newTargetType.Name));
            }

            var oldTarget = this.target as INotifyPropertyChanged;
            if (oldTarget != null)
                oldTarget.PropertyChanged -= this.PropertyChangedHandler;

            this.target = newTarget;

            var inpc = newTarget as INotifyPropertyChanged;
            if (this.guardPropertyGetter != null && inpc != null)
                inpc.PropertyChanged += this.PropertyChangedHandler;

            this.targetMethodInfo = targetMethodInfo;

            this.UpdateCanExecute();
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e.PropertyName) || e.PropertyName == this.GuardName)
            {
                this.UpdateCanExecute();
            }
        }

        private void UpdateCanExecute()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            if (this.target == null)
                return false;

            if (this.guardPropertyGetter == null)
                return true;

            return this.guardPropertyGetter();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            // This is not going to be called very often, so don't bother to generate a delegate, in the way that we do for the method guard
            if (this.target == null)
                throw new ArgumentException("Target not set");

            var parameters = this.targetMethodInfo.GetParameters().Length == 1 ? new[] { parameter } : null;
            this.targetMethodInfo.Invoke(this.target, parameters);
        }
    }
}
