using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Stylet.Xaml
{
    /// <summary>
    /// Turn a boolean value into a Visibility
    /// </summary>
    public class BoolToVisibilityConverter : DependencyObject, IValueConverter
    {
        public static readonly BoolToVisibilityConverter Instance = new BoolToVisibilityConverter();

        /// <summary>
        /// Visibility to use if value is true
        /// </summary>
        public Visibility TrueVisibility
        {
            get { return (Visibility)GetValue(TrueVisibilityProperty); }
            set { SetValue(TrueVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TrueVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrueVisibilityProperty =
            DependencyProperty.Register("TrueVisibility", typeof(Visibility), typeof(BoolToVisibilityConverter), new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Visibility to use if value is false
        /// </summary>
        public Visibility FalseVisibility
        {
            get { return (Visibility)GetValue(FalseVisibilityProperty); }
            set { SetValue(FalseVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FalseVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FalseVisibilityProperty =
            DependencyProperty.Register("FalseVisibility", typeof(Visibility), typeof(BoolToVisibilityConverter), new PropertyMetadata(Visibility.Collapsed));


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result;
            if (value == null)
                result = false;
            else if (value is bool)
                result = (bool)value;
            else if (value is IEnumerable)
                result = ((IEnumerable)value).GetEnumerator().MoveNext();
            else if (value.Equals(0) || value.Equals(0.0f) || value.Equals(0.0) || value.Equals(0u) || value.Equals(0m))
                result = false;
            else
                return null;

            return result ? this.TrueVisibility : this.FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("Can't ConvertBack on BoolToVisibilityConverter when TargetType is not bool");

            if (!(value is Visibility))
                return null;

            var vis = (Visibility)value;

            if (vis == this.TrueVisibility)
                return true;
            if (vis == this.FalseVisibility)
                return false;
            return null;
        }
    }
}
