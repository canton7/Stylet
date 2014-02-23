using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Stylet
{
    public class View : DependencyObject
    {
        static readonly ContentPropertyAttribute DefaultContentProperty = new ContentPropertyAttribute("Content");

        public static object GetTarget(DependencyObject obj)
        {
            return (object)obj.GetValue(TargetProperty);
        }

        public static void SetTarget(DependencyObject obj, object value)
        {
            obj.SetValue(TargetProperty, value);
        }

        // Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.RegisterAttached("Target", typeof(object), typeof(View), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));



        public static object GetModel(DependencyObject obj)
        {
            return (object)obj.GetValue(ModelProperty);
        }

        public static void SetModel(DependencyObject obj, object value)
        {
            obj.SetValue(ModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for Model.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.RegisterAttached("Model", typeof(object), typeof(View), new PropertyMetadata(null, (d, e) => IoC.Get<IViewManager>().OnModelChanged(d, e) ));


        public static void SetContentProperty(DependencyObject targetLocation, UIElement view)
        {
            var type = targetLocation.GetType();
            var contentProperty = Attribute.GetCustomAttributes(type, true).OfType<ContentPropertyAttribute>().FirstOrDefault() ?? DefaultContentProperty;

            type.GetProperty(contentProperty.Name).SetValue(targetLocation, view, null);
        }
    }
}
