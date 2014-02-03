using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace MicroMVVM
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
            if (valueService.TargetProperty is ICommand)
            {
                return new ActionCommand((FrameworkElement)valueService.TargetObject, this.Method);
            }
            else
            {
                var ec = new EventCommand((FrameworkElement)valueService.TargetObject, valueService.TargetProperty, this.Method);
                return ec.GetDelegate();
            }

        }
    }
}
