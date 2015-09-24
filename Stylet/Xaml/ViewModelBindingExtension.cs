using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Stylet.Xaml
{
    /// <summary>
    /// MarkupExtension which can retrieve the ViewModel for the current View, if available
    /// </summary>
    public class ViewModelBindingExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the path to the property
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the converter to use.
        /// </summary>
        public IValueConverter Converter { get; set; }

        /// <summary>
        /// Gets or sets the culture in which to evaluate the converter.
        /// </summary>
        public CultureInfo ConverterCulture { get; set; }

        /// <summary>
        /// Gets or sets the parameter to pass to the Converter.
        /// </summary>
        public object ConverterParameter { get; set; }

        /// <summary>
        /// Gets or sets the value to use when the binding is unable to return a value
        /// </summary>
        public object FallbackValue { get; set; }

        /// <summary>
        /// Gets or sets a string that specifies how to format the binding if it displays the bound value as a string
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// Gets or sets the value that is used in the target when the value of the source is null
        /// </summary>
        public object TargetNullValue { get; set; }

        /// <summary>
        /// Instantiates a new instance of the <see cref="ViewModelBindingExtension"/> class
        /// </summary>
        public ViewModelBindingExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Binding class with an initial path
        /// </summary>
        /// <param name="path">The initial Path for the binding</param>
        public ViewModelBindingExtension(string path)
        {
            this.Path = path;
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

            View.EnsureViewModelProxyValueSetUp(targetObjectAsDependencyObject);

            var binding = new Binding()
            {
                Source = targetObjectAsDependencyObject,
                Path = new PropertyPath("(0).(1)." + this.Path, View.ViewModelProxyProperty, BindingProxy.DataProperty),
                Mode = BindingMode.OneWay,
                Converter = this.Converter,
                ConverterCulture = this.ConverterCulture,
                ConverterParameter = this.ConverterParameter,
                FallbackValue = this.FallbackValue,
                StringFormat = this.StringFormat,
                TargetNullValue = this.TargetNullValue,
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
