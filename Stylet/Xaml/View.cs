using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Stylet.Xaml
{
    public class View : DependencyObject
    {
        private static readonly ContentPropertyAttribute defaultContentProperty = new ContentPropertyAttribute("Content");
        public static IViewManager ViewManager;

        public static object GetActionTarget(DependencyObject obj)
        {
            return (object)obj.GetValue(ActionTargetProperty);
        }

        public static void SetActionTarget(DependencyObject obj, object value)
        {
            obj.SetValue(ActionTargetProperty, value);
        }

        public static readonly DependencyProperty ActionTargetProperty =
            DependencyProperty.RegisterAttached("ActionTarget", typeof(object), typeof(View), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static object GetModel(DependencyObject obj)
        {
            return (object)obj.GetValue(ModelProperty);
        }

        public static void SetModel(DependencyObject obj, object value)
        {
            obj.SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.RegisterAttached("Model", typeof(object), typeof(View), new PropertyMetadata(null, (d, e) => ViewManager.OnModelChanged(d, e) ));


        public static void SetContentProperty(DependencyObject targetLocation, UIElement view)
        {
            var type = targetLocation.GetType();
            var contentProperty = Attribute.GetCustomAttributes(type, true).OfType<ContentPropertyAttribute>().FirstOrDefault() ?? defaultContentProperty;

            type.GetProperty(contentProperty.Name).SetValue(targetLocation, view, null);
        }
    }
}
