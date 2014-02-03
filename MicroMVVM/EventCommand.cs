using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MicroMVVM
{
    public class EventCommand
    {
        private FrameworkElement subject;
        private object targetProperty;
        private string methodName;

        public EventCommand(FrameworkElement subject, object targetProperty, string methodName)
        {
            this.subject = subject;
            this.targetProperty = targetProperty;
            this.methodName = methodName;
        }

        public Delegate GetDelegate()
        {
            Delegate del = null;

            var eventInfo = this.targetProperty as EventInfo;
            var methodInfo = this.GetType().GetMethod("InvokeCommand", BindingFlags.NonPublic | BindingFlags.Instance);

            Type parameterType = null;
            if (eventInfo != null)
                parameterType = eventInfo.EventHandlerType;

            if (parameterType != null)
            {
                del = Delegate.CreateDelegate(parameterType, this, methodInfo);
            }

            return del;
        }

        private void InvokeCommand(object sender, RoutedEventArgs e)
        {
            var target = View.GetTarget(this.subject);
            if (target == null)
                throw new Exception("Target not set");

            var methodInfo = target.GetType().GetMethod(this.methodName);
            if (methodInfo == null)
                throw new Exception(String.Format("Unable to find method {0} on {1}", this.methodName, target.GetType().Name));

            var parameters = methodInfo.GetParameters().Length == 1 ? new object[] { e } : null;
            methodInfo.Invoke(target, parameters);
        }
    }
}
