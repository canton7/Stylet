using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Stylet.Xaml
{
    /// <summary>
    /// Converter to compare a number of values, and return true (or false if Invert is true) if they are all equal
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Don't agree with prefixing static method calls with the class name")]
    public class EqualityConverter : DependencyObject, IMultiValueConverter
    {
        /// <summary>
        /// Singleton instance of this converter. Usage: Converter="{x:Static s:EqualityConverter.Instance}"
        /// </summary>
        public static readonly EqualityConverter Instance = new EqualityConverter();

        /// <summary>
        /// Gets or sets a value indicating whether to return false, instead of true, if call values are equal
        /// </summary>
        public bool Invert
        {
            get { return (bool)this.GetValue(InvertProperty); }
            set { this.SetValue(InvertProperty, value); }
        }

        /// <summary>
        /// Property specifying whether the output should be inverted
        /// </summary>
        public static readonly DependencyProperty InvertProperty =
            DependencyProperty.Register("Invert", typeof(bool), typeof(EqualityConverter), new PropertyMetadata(false));

        /// <summary>
        /// Perform the conversion
        /// </summary>
        /// <param name="values">
        ///     Array of values, as produced by source bindings.
        ///     System.Windows.DependencyProperty.UnsetValue may be passed to indicate that
        ///     the source binding has no value to provide for conversion.
        /// </param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">converter parameter</param>
        /// <param name="culture">culture information</param>
        /// <returns>Converted values</returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return null;
            var first = values.FirstOrDefault();
            var result = values.Skip(1).All(x => x.Equals(first));
            return this.Invert ? !result : result;
        }

        /// <summary>
        /// Perform the reverse convesion. Not implemented.
        /// </summary>
        /// <param name="value">value, as produced by target</param>
        /// <param name="targetTypes">
        ///     Array of target types; array length indicates the number and types
        ///     of values suggested for Convert to return.
        /// </param>
        /// <param name="parameter">converter parameter</param>
        /// <param name="culture">culture information</param>
        /// <returns>Converted back values</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
