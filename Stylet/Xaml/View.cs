using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Reflection;
using Stylet.Logging;

namespace Stylet.Xaml
{
    /// <summary>
    /// Holds attached properties relating to various bits of the View which are used by Stylet
    /// </summary>
    public static class View
    {
        private static readonly ILogger logger = LogManager.GetLogger(typeof(View));

        internal const string ViewManagerResourceKey = "b9a38199-8cb3-4103-8526-c6cfcd089df7";

        internal const string ActionTargetProxyResourceKey = "8b7cb732-8a14-4813-a580-b1f3cccea7b7";

        internal const string DataContextProxyResourceKey = "982a3cb4-68b8-464f-9f65-8835d86d94dd";

        /// <summary>
        /// Initial value of the ActionTarget property.
        /// This can be used as a marker - if the property has this value, it hasn't yet been assigned to anything else.
        /// </summary>
        public static readonly object InitialActionTarget = new object();

        /// <summary>
        /// Get the ActionTarget associated with the given object
        /// </summary>
        /// <param name="obj">Object to fetch the ActionTarget for</param>
        /// <returns>ActionTarget associated with the given object</returns>
        public static object GetActionTarget(DependencyObject obj)
        {
            var actionTarget = obj.GetValue(ActionTargetProperty);

            if (actionTarget == InitialActionTarget)
            {
                var frameworkElement = obj as FrameworkElement;
                if (frameworkElement != null)
                {
                    var bindingProxy = frameworkElement.TryFindResource(ActionTargetProxyResourceKey) as BindingProxy;
                    if (bindingProxy != null)
                    {
                        logger.Info("ActionTarget not set on object {0}, but a BindingProxy containing an ActionTarget was, so using that", obj);
                        actionTarget = bindingProxy.Data;
                    }
                }
            }

            return actionTarget;
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
            DependencyProperty.RegisterAttached("ActionTarget", typeof(object), typeof(View), new FrameworkPropertyMetadata(InitialActionTarget, FrameworkPropertyMetadataOptions.Inherits, (d, e) =>
            {
                // Also set a binding proxy if we can, in case there's something weird in the way
                var frameworkElement = d as FrameworkElement;
                if (frameworkElement == null)
                    return;

                var bindingProxy = new BindingProxy()
                {
                    Data = e.NewValue,
                };
                bindingProxy.Freeze();
                frameworkElement.Resources[ActionTargetProxyResourceKey] = bindingProxy;
            }));

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
                var viewManager = ((FrameworkElement)d).TryFindResource(ViewManagerResourceKey) as IViewManager;

                if (viewManager == null)
                {
                    if (Execute.InDesignMode)
                    {
                        var bindingExpression = BindingOperations.GetBindingExpression(d, ModelProperty);
                        string text;
                        if (bindingExpression == null)
                            text = "View for [Broken Binding]";
                        else if (bindingExpression.ResolvedSourcePropertyName == null)
                            text = String.Format("View for child ViewModel on {0}", bindingExpression.DataItem.GetType().Name);
                        else
                            text = String.Format("View for {0}.{1}", bindingExpression.DataItem.GetType().Name, bindingExpression.ResolvedSourcePropertyName);
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
                    var newValue = e.NewValue == defaultModelValue ? null : e.NewValue;
                    viewManager.OnModelChanged(d, e.OldValue, newValue);
                }
            }));


        internal static void SetDataContext(FrameworkElement obj, object value)
        {
            obj.DataContext = value;
            var bindingProxy = new BindingProxy()
            {
                Data = value,
            };
            obj.Resources[DataContextProxyResourceKey] = bindingProxy;
        }

        public static bool GetRestoreDataContext(DependencyObject obj)
        {
            return (bool)obj.GetValue(RestoreDataContextProperty);
        }

        public static void SetRestoreDataContext(DependencyObject obj, bool value)
        {
            obj.SetValue(RestoreDataContextProperty, value);
        }

        public static readonly DependencyProperty RestoreDataContextProperty =
            DependencyProperty.RegisterAttached("RestoreDataContext", typeof(bool), typeof(View), new PropertyMetadata(false, (d, e) =>
            {
                if (!(e.NewValue is bool) || !(bool)e.NewValue)
                    return;

                var frameworkElement = d as FrameworkElement;
                if (frameworkElement == null)
                    return;

                var bindingProxy = frameworkElement.Resources[DataContextProxyResourceKey] as BindingProxy;
                if (bindingProxy == null)
                    return;

                frameworkElement.DataContext = bindingProxy.Data;
            }));

        /// <summary>
        /// Helper to set the Content property of a given object to a particular View
        /// </summary>
        /// <param name="targetLocation">Object to set the Content property on</param>
        /// <param name="view">View to set as the object's Content</param>
        public static void SetContentProperty(DependencyObject targetLocation, UIElement view)
        {
            var type = targetLocation.GetType();
            var attribute = type.GetCustomAttribute<ContentPropertyAttribute>();
            // No attribute? Try a property called 'Content'...
            string propertyName = attribute != null ? attribute.Name : "Content";
            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException(String.Format("Unable to find a Content property on type {0}. Make sure you're using 's:View.Model' on a suitable container, e.g. a ContentControl", type.Name));
            property.SetValue(targetLocation, view);
        }
    }
}
