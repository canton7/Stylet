using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Stylet.Xaml
{
    /// <summary>
    /// Converter to compare a number of values, and return true (or false if Invert is true) if they are all equal
    /// </summary>
    public class EqualityConverter : DependencyObject, IMultiValueConverter
    {
        /// <summary>
        /// Singleton instance of this converter. Usage: Converter="{x:Static s:EqualityConverter.Instance}"
        /// </summary>
        public static readonly EqualityConverter Instance = new EqualityConverter();

        /// <summary>
        /// Return false, instead of true, if call values are equal
        /// </summary>
        public bool Invert
        {
            get { return (bool)GetValue(InvertProperty); }
            set { SetValue(InvertProperty, value); }
        }

        /// <summary>
        /// Property specifying whether the output should be inverted
        /// </summary>
        public static readonly DependencyProperty InvertProperty =
            DependencyProperty.Register("Invert", typeof(bool), typeof(EqualityConverter), new PropertyMetadata(false));

        /// <summary>
        /// Perform the conversion
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return null;
            var first = values.FirstOrDefault();
            var result = values.Skip(1).All(x => first.Equals(x));
            return this.Invert ? !result : result;
        }

        /// <summary>
        /// Perform the reverse convesion. Not implemented.
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
