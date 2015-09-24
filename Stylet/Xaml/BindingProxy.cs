using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Stylet.Xaml
{
    internal class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));        
    }

    /// <summary>
    /// Converter which extracts the 'Data' property from a BindingProxy.
    /// </summary>
    internal class BindingProxyToValueConverter : IValueConverter
    {
        public static readonly BindingProxyToValueConverter Instance = new BindingProxyToValueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var proxy = value as BindingProxy;
            if (proxy != null)
                return proxy.Data;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
