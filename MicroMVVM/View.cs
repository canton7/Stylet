using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MicroMVVM
{
    public class View : DependencyObject
    {
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
            DependencyProperty.RegisterAttached("Model", typeof(object), typeof(View), new PropertyMetadata(null, OnModelChanged));

        
        private static void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;

            if (e.NewValue != null)
            {
                var view = ViewLocator.LocateForModel(e.NewValue);
            }
        }
    }
}
