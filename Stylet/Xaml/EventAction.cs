using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    /// <summary>
    /// Created by ActionExtension, this can return a delegate suitable adding binding to an event, and can call a method on the View.ActionTarget
    /// </summary>
    public class EventAction
    {
        /// <summary>
        /// View whose View.ActionTarget we watch
        /// </summary>
        private FrameworkElement subject;

        /// <summary>
        /// Property on the WPF element we're returning a delegate for
        /// </summary>
        private EventInfo targetProperty;

        /// <summary>
        /// The MyMethod in {s:Action MyMethod}, this is what we call when the event's fired
        /// </summary>
        private string methodName;

        /// <summary>
        /// MethodInfo for the method to call. This has to exist, or we throw a wobbly
        /// </summary>
        private MethodInfo targetMethodInfo;

        private object target;

        /// <summary>
        /// Create a new EventAction
        /// </summary>
        /// <param name="subject">View whose View.ActionTarget we watch</param>
        /// <param name="targetProperty">Property on the WPF element we're returning a delegate for</param>
        /// <param name="methodName">The MyMethod in {s:Action MyMethod}, this is what we call when the event's fired</param>
        public EventAction(FrameworkElement subject, EventInfo targetProperty, string methodName)
        {
            this.subject = subject;
            this.targetProperty = targetProperty;
            this.methodName = methodName;

            // Observe the View.ActionTarget for changes, and re-bind the guard property and MethodInfo if it changes
            DependencyPropertyDescriptor.FromProperty(View.ActionTargetProperty, typeof(View)).AddValueChanged(this.subject, (o, e) => this.UpdateMethod());
        }

        private void UpdateMethod()
        {
            var newTarget = View.GetActionTarget(this.subject);
            MethodInfo targetMethodInfo = null;

            if (newTarget != null)
            {
                var newTargetType = newTarget.GetType();
                targetMethodInfo = newTargetType.GetMethod(this.methodName);
                if (targetMethodInfo == null)
                    throw new ArgumentException(String.Format("Unable to find method {0} on {1}", this.methodName, newTargetType.Name));

                var methodParameters = targetMethodInfo.GetParameters();
                if (methodParameters.Length > 1 || (methodParameters.Length == 1 && !typeof(RoutedEventArgs).IsAssignableFrom(methodParameters[0].ParameterType)))
                    throw new ArgumentException(String.Format("Method {0} on {1} must have zero parameters, or a single parameter accepting a RoutedEventArgs", this.methodName, newTargetType.Name));
            }

            this.target = newTarget;
            this.targetMethodInfo = targetMethodInfo;
        }

        /// <summary>
        /// Return a delegate which can be added to the targetProperty
        /// </summary>
        public Delegate GetDelegate()
        {
            var methodInfo = this.GetType().GetMethod("InvokeCommand", BindingFlags.NonPublic | BindingFlags.Instance);

            var parameterType = this.targetProperty.EventHandlerType;
            return Delegate.CreateDelegate(parameterType, this, methodInfo);
        }

        private void InvokeCommand(object sender, RoutedEventArgs e)
        {
            var parameters = this.targetMethodInfo.GetParameters().Length == 1 ? new object[] { e } : null;
            this.targetMethodInfo.Invoke(target, parameters);
        }
    }
}
