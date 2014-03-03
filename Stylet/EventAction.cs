using System;
using System.Collections.Generic;
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
            var target = View.GetActionTarget(this.subject);
            if (target == null)
                return;

            var methodInfo = target.GetType().GetMethod(this.methodName);
            if (methodInfo == null)
                throw new Exception(String.Format("Unable to find method {0} on {1}", this.methodName, target.GetType().Name));

            var parameters = methodInfo.GetParameters().Length == 1 ? new object[] { e } : null;
            methodInfo.Invoke(target, parameters);
        }
    }
}
