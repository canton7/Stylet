using System;
using System.Drawing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Stylet.Xaml
{
    /// <summary>
    /// Converter to take an Icon, and convert it to a BitmapSource
    /// </summary>
    public class IconToBitmapSourceConverter : IValueConverter
    {
        /// <summary>
        /// Singleton instance of this converter. Usage e.g. Converter="{x:Static s:IconToBitmapSourceConverter.Instance}"
        /// </summary>
        public static IconToBitmapSourceConverter Instance = new IconToBitmapSourceConverter();

        /// <summary>
        /// Converts a value
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var icon = value as Icon;
            if (icon == null)
                return null;
            var bs = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bs;
        }

        /// <summary>
        /// Converts a value back. Not implemented.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
