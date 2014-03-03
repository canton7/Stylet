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
    /// <summary>
    /// MarkupExtension used for binding Commands and Events to methods on the View.ActionTarget
    /// </summary>
    public class ActionExtension : MarkupExtension
    {
        /// <summary>
        /// Name of the method to call
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Create a new ActionExtension
        /// </summary>
        /// <param name="method">Name of the method to call</param>
        public ActionExtension(string method)
        {
            this.Method = method;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var valueService = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            // Seems this is the case when we're in a template. We'll get called again properly in a second.
            // http://social.msdn.microsoft.com/Forums/vstudio/en-US/a9ead3d5-a4e4-4f9c-b507-b7a7d530c6a9/gaining-access-to-target-object-instead-of-shareddp-in-custom-markupextensions-providevalue-method?forum=wpf
            if (!(valueService.TargetObject is FrameworkElement))
                return this;

            var propertyAsDependencyProperty = valueService.TargetProperty as DependencyProperty;
            if (propertyAsDependencyProperty != null && propertyAsDependencyProperty.PropertyType == typeof(ICommand))
            {
                return new CommandAction((FrameworkElement)valueService.TargetObject, this.Method);
            }

            var propertyAsEventInfo = valueService.TargetProperty as EventInfo;
            if (propertyAsEventInfo != null)
            {
                var ec = new EventAction((FrameworkElement)valueService.TargetObject, propertyAsEventInfo, this.Method);
                return ec.GetDelegate();
            }
                
            throw new ArgumentException("Can only use ActionExtension with a Command property or an event handler");
        }
    }
}
