using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Stylet.Xaml
{
    /// <summary>
    /// Holds attached properties relating to various bits of the View which are used by Stylet
    /// </summary>
    public class View : DependencyObject
    {
        /// <summary>
        /// Initial value of the ActionTarget property.
        /// This can be used as a marker - if the property has this value, it hasn't yet been assigned to anything else.
        /// </summary>
        public static readonly object InitialActionTarget = new object();

        private static readonly ContentPropertyAttribute defaultContentProperty = new ContentPropertyAttribute("Content");

        /// <summary>
        /// Gets or sets the <see cref="IViewManager"/> to be used. This should be set by the Bootstrapper.
        /// </summary>
        public static IViewManager ViewManager { get; set; }

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

        private static readonly object defaultModelValue = new object();

        /// <summary>
        /// Property specifying the ViewModel currently associated with a given object
        /// </summary>
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.RegisterAttached("Model", typeof(object), typeof(View), new PropertyMetadata(defaultModelValue, (d, e) =>
            {
                if (ViewManager == null)
                {
                    if (Execute.InDesignMode)
                    {
                        var bindingExpression = BindingOperations.GetBindingExpression(d, View.ModelProperty);
                        string text;
                        if (bindingExpression == null)
                            text = "View for [Broken Binding]";
                        else if (bindingExpression.ResolvedSourcePropertyName == null)
                            text = String.Format("View for child ViewModel on {0}", bindingExpression.DataItem.GetType().Name);
                        else
                            text = String.Format("View for {0}.{1}", bindingExpression.DataItem.GetType().Name, bindingExpression.ResolvedSourcePropertyName);
                        View.SetContentProperty(d, new System.Windows.Controls.TextBlock() { Text = text });
                    }
                    else
                    {
                        throw new InvalidOperationException("View.ViewManager is unassigned. This should have been set by the Bootstrapper");
                    }
                }
                else
                {
                    // It appears we can be reset to the default value on destruction
                    var newValue = e.NewValue == defaultModelValue ? null : e.NewValue;
                    ViewManager.OnModelChanged(d, e.OldValue, newValue);
                }
            }));

        /// <summary>
        /// Helper to set the Content property of a given object to a particular View
        /// </summary>
        /// <param name="targetLocation">Object to set the Content property on</param>
        /// <param name="view">View to set as the object's Content</param>
        public static void SetContentProperty(DependencyObject targetLocation, UIElement view)
        {
            var type = targetLocation.GetType();
            var contentProperty = Attribute.GetCustomAttributes(type, true).OfType<ContentPropertyAttribute>().FirstOrDefault() ?? defaultContentProperty;

            type.GetProperty(contentProperty.Name).SetValue(targetLocation, view, null);
        }

        // Stop someone from instantiating us
        private View()
        { }
    }
}
