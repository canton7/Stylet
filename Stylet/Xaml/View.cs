using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Stylet.Xaml
{
    /// <summary>
    /// Holds attached properties relating to various bits of the View which are used by Stylet
    /// </summary>
    public static class View
    {
        /// <summary>
        /// Key which will be used to retrieve the ViewManager associated with the current application, from application's resources
        /// </summary>
        public const string ViewManagerResourceKey = "b9a38199-8cb3-4103-8526-c6cfcd089df7";

        /// <summary>
        /// Initial value of the ActionTarget property.
        /// This can be used as a marker - if the property has this value, it hasn't yet been assigned to anything else.
        /// </summary>
        public static readonly object InitialActionTarget = new();

        /// <summary>
        /// Get the ActionTarget associated with the given object
        /// </summary>
        /// <param name="obj">Object to fetch the ActionTarget for</param>
        /// <returns>ActionTarget associated with the given object</returns>
        public static object GetActionTarget(DependencyObject obj)
        {
            return obj.GetValue(ActionTargetProperty);
        }

        /// <summary>
        /// Set the ActionTarget associated with the given object
        /// </summary>
        /// <param name="obj">Object to set the ActionTarget for</param>
        /// <param name="value">Value to set the ActionTarget to</param>
        public static void SetActionTarget(DependencyObject obj, object value)
        {
            obj.SetValue(ActionTargetProperty, value);
        }

        /// <summary>
        /// The object's ActionTarget. This is used to determine what object to call Actions on by the ActionExtension markup extension.
        /// </summary>
        public static readonly DependencyProperty ActionTargetProperty =
            DependencyProperty.RegisterAttached("ActionTarget", typeof(object), typeof(View), new FrameworkPropertyMetadata(InitialActionTarget, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Fetch the ViewModel currently associated with a given object
        /// </summary>
        /// <param name="obj">Object to fetch the ViewModel for</param>
        /// <returns>ViewModel currently associated with the given object</returns>
        public static object GetModel(DependencyObject obj)
        {
            return obj.GetValue(ModelProperty);
        }

        /// <summary>
        /// Set the ViewModel currently associated with a given object
        /// </summary>
        /// <param name="obj">Object to set the ViewModel for</param>
        /// <param name="value">ViewModel to set</param>
        public static void SetModel(DependencyObject obj, object value)
        {
            obj.SetValue(ModelProperty, value);
        }

        private static readonly object defaultModelValue = new();

        /// <summary>
        /// Property specifying the ViewModel currently associated with a given object
        /// </summary>
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.RegisterAttached("Model", typeof(object), typeof(View), new PropertyMetadata(defaultModelValue, (d, e) =>
            {
                if (((FrameworkElement)d).TryFindResource(ViewManagerResourceKey) is not IViewManager viewManager)
                {
                    if (Execute.InDesignMode)
                    {
                        BindingExpression bindingExpression = BindingOperations.GetBindingExpression(d, ModelProperty);
                        string text;
                        if (bindingExpression == null)
                            text = "View for [Broken Binding]";
                        else if (bindingExpression.ResolvedSourcePropertyName == null)
                            text = string.Format("View for child ViewModel on {0}", bindingExpression.DataItem.GetType().Name);
                        else
                            text = string.Format("View for {0}.{1}", bindingExpression.DataItem.GetType().Name, bindingExpression.ResolvedSourcePropertyName);
                        SetContentProperty(d, new System.Windows.Controls.TextBlock() { Text = text });
                    }
                    else
                    {
                        throw new InvalidOperationException("The ViewManager resource is unassigned. This should have been set by the Bootstrapper");
                    }
                }
                else
                {
                    // It appears we can be reset to the default value on destruction
                    object newValue = e.NewValue == defaultModelValue ? null : e.NewValue;
                    viewManager.OnModelChanged(d, e.OldValue, newValue);
                }
            }));

        /// <summary>
        /// Helper to set the Content property of a given object to a particular View
        /// </summary>
        /// <param name="targetLocation">Object to set the Content property on</param>
        /// <param name="view">View to set as the object's Content</param>
        public static void SetContentProperty(DependencyObject targetLocation, UIElement view)
        {
            Type type = targetLocation.GetType();
            ContentPropertyAttribute attribute = type.GetCustomAttribute<ContentPropertyAttribute>();
            // No attribute? Try a property called 'Content'...
            string propertyName = attribute != null ? attribute.Name : "Content";
            PropertyInfo property = type.GetProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException(string.Format("Unable to find a Content property on type {0}. Make sure you're using 's:View.Model' on a suitable container, e.g. a ContentControl", type.Name));
            property.SetValue(targetLocation, view);
        }
    }
}
