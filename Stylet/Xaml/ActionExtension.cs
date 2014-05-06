using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace Stylet.Xaml
{
    public enum ActionUnavailableBehaviour
    {
        Enable,
        Disable,
        Throw
    };

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
        /// Behaviour if the View.ActionTarget is nulil
        /// </summary>
        public ActionUnavailableBehaviour? NullTarget { get; set; }

        /// <summary>
        /// Behaviour if the action itself isn't found on the View.ActionTarget
        /// </summary>
        public ActionUnavailableBehaviour? ActionNotFound { get; set; }

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
            if (!(valueService.TargetObject is DependencyObject))
                return this;

            var propertyAsDependencyProperty = valueService.TargetProperty as DependencyProperty;
            if (propertyAsDependencyProperty != null && propertyAsDependencyProperty.PropertyType == typeof(ICommand))
            {
                return new CommandAction((DependencyObject)valueService.TargetObject, this.Method, this.NullTarget.GetValueOrDefault(ActionUnavailableBehaviour.Disable), this.ActionNotFound.GetValueOrDefault(ActionUnavailableBehaviour.Throw));
            }

            var propertyAsEventInfo = valueService.TargetProperty as EventInfo;
            if (propertyAsEventInfo != null)
            {
                var ec = new EventAction((DependencyObject)valueService.TargetObject, propertyAsEventInfo, this.Method, this.NullTarget.GetValueOrDefault(ActionUnavailableBehaviour.Enable), this.ActionNotFound.GetValueOrDefault(ActionUnavailableBehaviour.Throw));
                return ec.GetDelegate();
            }
                
            throw new ArgumentException("Can only use ActionExtension with a Command property or an event handler");
        }
    }
}
