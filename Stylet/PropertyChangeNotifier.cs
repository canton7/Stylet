using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace Stylet
{    
    /// <summary>
    /// DependencyProperty change notifier which does not root the DependencyObject
    /// </summary>
    // Adapted from https://agsmith.wordpress.com/2008/04/07/propertydescriptor-addvaluechanged-alternative/
    public class PropertyChangeNotifier : DependencyObject, IDisposable
    {
        /// <summary>
        /// Watch for changes of the given property on the given propertySource
        /// </summary>
        /// <param name="propertySource">Object to observe a property on</param>
        /// <param name="property">Property on the object to observe</param>
        /// <param name="handler">Handler to invoke when the property changes</param>
        /// <returns>The constructed PropertyChangeNotifier</returns>
        public static PropertyChangeNotifier AddValueChanged(DependencyObject propertySource, PropertyPath property, PropertyChangedCallback handler)
        {
            return new PropertyChangeNotifier(propertySource, property, handler);
        }

        /// <summary>
        /// Watch for changes of the given property on the given propertySource
        /// </summary>
        /// <param name="propertySource">Object to observe a property on</param>
        /// <param name="property">Property on the object to observe</param>
        /// <param name="handler">Handler to invoke when the property changes</param>
        /// <returns>The constructed PropertyChangeNotifier</returns>
        public static PropertyChangeNotifier AddValueChanged(DependencyObject propertySource, DependencyProperty property, PropertyChangedCallback handler)
        {
            return AddValueChanged(propertySource, new PropertyPath(property), handler);
        }

        private readonly PropertyChangedCallback handler;
        private readonly WeakReference<DependencyObject> propertySource;

        private PropertyChangeNotifier(DependencyObject propertySource, PropertyPath property, PropertyChangedCallback handler)
        {
            if (propertySource == null)
                throw new ArgumentNullException("propertySource");
            if (property == null)
                throw new ArgumentNullException("property");
            if (handler == null)
                throw new ArgumentNullException("handler");

            this.propertySource = new WeakReference<DependencyObject>(propertySource);
            this.handler = handler;

            var binding = new Binding()
            {
                Path = property,
                Mode = BindingMode.OneWay,
                Source = propertySource
            };
            BindingOperations.SetBinding(this, ValueProperty, binding);
        }

        private void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            // Target *should* never be null at this point...
            DependencyObject propertySource;
            if (!this.propertySource.TryGetTarget(out propertySource))
                Debug.Assert(false);
            this.handler(propertySource, e);
        }

        private static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertyChangeNotifier), new FrameworkPropertyMetadata(null, (d, e) =>
            {
                ((PropertyChangeNotifier)d).OnValueChanged(e);
            }));

        /// <summary>
        /// Releases the binding
        /// </summary>
        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ValueProperty);
        }
    }
}
