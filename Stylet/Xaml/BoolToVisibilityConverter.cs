using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;

namespace Stylet.Xaml
{
    /// <summary>
    /// Turn a boolean value into a Visibility
    /// </summary>
    public class BoolToVisibilityConverter : DependencyObject, IValueConverter
    {
        /// <summary>
        /// Singleton instance of this converter. Usage e.g. Converter="{x:Static s:BoolToVisibilityConverter.Instance}"
        /// </summary>
        public static readonly BoolToVisibilityConverter Instance = new BoolToVisibilityConverter();

        /// <summary>
        /// Visibility to use if value is true
        /// </summary>
        public Visibility TrueVisibility
        {
            get { return (Visibility)GetValue(TrueVisibilityProperty); }
            set { SetValue(TrueVisibilityProperty, value); }
        }

        /// <summary>
        /// Property specifying the visibility to return when the parameter is true
        /// </summary>
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

        /// <summary>
        /// Property specifying the visibility to return when the parameter is false
        /// </summary>
        public static readonly DependencyProperty FalseVisibilityProperty =
            DependencyProperty.Register("FalseVisibility", typeof(Visibility), typeof(BoolToVisibilityConverter), new PropertyMetadata(Visibility.Collapsed));


        /// <summary>
        /// Perform the conversion
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result;
            if (value == null)
                result = false;
            else if (value is bool)
                result = (bool)value;
            else if (value is IEnumerable)
                result = ((IEnumerable)value).GetEnumerator().MoveNext();
            else if (value.Equals(System.Convert.ChangeType((object)0, value.GetType())))
                result = false;
            else
                result = true; // Not null, didn't meet any other falsy behaviour

            return result ? this.TrueVisibility : this.FalseVisibility;
        }

        /// <summary>
        /// Perform the inverse conversion. Only valid if the value is bool
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new ArgumentException("Can't ConvertBack on BoolToVisibilityConverter when TargetType is not bool");

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
