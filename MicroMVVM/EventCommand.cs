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

        public EventCommand(FrameworkElement subject, object targetProperty)
        {
            this.subject = subject;
            this.targetProperty = targetProperty;
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

        }
    }
}
