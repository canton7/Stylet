using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Stylet.Xaml
{
    /// <summary>
    /// MarkupExtension which can retrieve the ViewModel for the current View, if available
    /// </summary>
    public class ViewModelExtension : MarkupExtension
    {
        /// <summary>
        /// Instantiates a new instsance of the <see cref="ViewModelExtension"/> class
        /// </summary>
        public ViewModelExtension()
        {
        }

        /// <summary>
        ///  When implemented in a derived class, returns an object that is provided as the
        ///  value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>The object value to set on the property where the extension is applied.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var valueService = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            var targetObjectAsDependencyObject = valueService.TargetObject as DependencyObject;
            if (targetObjectAsDependencyObject == null)
                return this;

            return View.GetBindingToViewModel(targetObjectAsDependencyObject).ProvideValue(serviceProvider);
        }
    }
}
