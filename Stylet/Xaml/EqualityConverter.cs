using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Stylet.Xaml
{
    public class EqualityConverter : DependencyObject, IMultiValueConverter
    {
        public static readonly EqualityConverter Instance = new EqualityConverter();

        public bool Invert
        {
            get { return (bool)GetValue(InvertProperty); }
            set { SetValue(InvertProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Invert.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvertProperty =
            DependencyProperty.Register("Invert", typeof(bool), typeof(EqualityConverter), new PropertyMetadata(false));

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var first = values.FirstOrDefault();
            var result = values.Skip(1).All(x => first.Equals(x));
            return this.Invert ? !result : result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
