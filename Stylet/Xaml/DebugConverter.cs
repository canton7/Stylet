using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Stylet.Xaml
{
    /// <summary>
    /// Converter which passes through values, but uses Debug.WriteLine to log them. Useful for debugging
    /// </summary>
    public class DebugConverter : DependencyObject, IValueConverter
    {
        public static readonly DebugConverter Instance = new DebugConverter();

        /// <summary>
        /// Category to use with Debug.WriteLine
        /// </summary>
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(DebugConverter), new PropertyMetadata("DebugConverter"));

        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                Debug.WriteLine(String.Format("Convert: Value = '{0}' TargetType = '{1}'", value, targetType), this.Name);
            else
                Debug.WriteLine(String.Format("Convert: Value = '{0}' TargetType = '{1}' Parameter = '{2}'", value, targetType, parameter), this.Name);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                Debug.WriteLine(String.Format("ConvertBack: Value = '{0}' TargetType = '{1}'", value, targetType), this.Name);
            else
                Debug.WriteLine(String.Format("ConvertBack: Value = '{0}' TargetType = '{1}' Parameter = '{2}'", value, targetType, parameter), this.Name);

            return value;
        }
    }
}
