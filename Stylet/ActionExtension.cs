using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace Stylet
{
    public class ActionExtension : MarkupExtension
    {
        public string Method { get; set; }

        public ActionExtension(string method)
        {
            this.Method = method;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var valueService = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            var propertyAsDependencyProperty = valueService.TargetProperty as DependencyProperty;
            if (propertyAsDependencyProperty != null && propertyAsDependencyProperty.PropertyType == typeof(ICommand))
            {
                return new ActionCommand((FrameworkElement)valueService.TargetObject, this.Method);
            }

            var propertyAsEventInfo = valueService.TargetProperty as EventInfo;
            if (propertyAsEventInfo != null)
            {
                var ec = new EventCommand((FrameworkElement)valueService.TargetObject, propertyAsEventInfo, this.Method);
                return ec.GetDelegate();
            }
                
            throw new ArgumentException("Can only use ActionExtension with a Command property or an event handler");
        }
    }
}
