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
    public class DependencyPropertyChangeNotifier : DependencyObject, IDisposable
    {
        /// <summary>
        /// Watch for changes of the given property on the given propertySource
        /// </summary>
        /// <param name="propertySource">Object to observe a property on</param>
        /// <param name="property">Property on the object to observe</param>
        /// <param name="handler">Handler to invoke when the property changes</param>
        /// <returns>The constructed PropertyChangeNotifier</returns>
        public static DependencyPropertyChangeNotifier AddValueChanged(DependencyObject propertySource, PropertyPath property, PropertyChangedCallback handler)
        {
            return new DependencyPropertyChangeNotifier(propertySource, property, handler);
        }

        /// <summary>
        /// Watch for changes of the given property on the given propertySource
        /// </summary>
        /// <param name="propertySource">Object to observe a property on</param>
        /// <param name="property">Property on the object to observe</param>
        /// <param name="handler">Handler to invoke when the property changes</param>
        /// <returns>The constructed PropertyChangeNotifier</returns>
        public static DependencyPropertyChangeNotifier AddValueChanged(DependencyObject propertySource, DependencyProperty property, PropertyChangedCallback handler)
        {
            if (property == null)
                throw new ArgumentNullException("property");
            return AddValueChanged(propertySource, new PropertyPath(property), handler);
        }

        private PropertyChangedCallback handler;
        private readonly WeakReference<DependencyObject> propertySource;

        private DependencyPropertyChangeNotifier(DependencyObject propertySource, PropertyPath property, PropertyChangedCallback handler)
        {
            if (propertySource == null)
                throw new ArgumentNullException("propertySource");
            if (property == null)
                throw new ArgumentNullException("property");
            if (handler == null)
                throw new ArgumentNullException("handler");

            this.propertySource = new WeakReference<DependencyObject>(propertySource);

            var binding = new Binding()
            {
                Path = property,
                Mode = BindingMode.OneWay,
                Source = propertySource
            };
            BindingOperations.SetBinding(this, ValueProperty, binding);

            // Needs to be set after binding set, so it doesn't catch the initial property set
            this.handler = handler;
        }

        private void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            // This happens on the firsrt invocation ever, when the initial value is set
            // and on disposal
            if (this.handler == null)
                return;

            // Target *should* never be null at this point...
            DependencyObject propertySource = null;
            this.propertySource.TryGetTarget(out propertySource);
            Debug.Assert(propertySource != null);
            this.handler(propertySource, e);
        }

        private static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(DependencyPropertyChangeNotifier), new FrameworkPropertyMetadata(null, (d, e) =>
            {
                ((DependencyPropertyChangeNotifier)d).OnValueChanged(e);
            }));

        /// <summary>
        /// Releases the binding
        /// </summary>
        public void Dispose()
        {
            this.handler = null; // Otherwise it's called as the binding is unset
            BindingOperations.ClearBinding(this, ValueProperty);
        }
    }
}
