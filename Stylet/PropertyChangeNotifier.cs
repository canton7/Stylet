using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <param name="notifier">Handler to invoke when the property changes</param>
        /// <returns>The constructed PropertyChangeNotifier</returns>
        public static PropertyChangeNotifier AddValueChanged(DependencyObject propertySource, PropertyPath property, PropertyChangedCallback notifier)
        {
            var not = new PropertyChangeNotifier(propertySource, property);
            not.ValueChanged += notifier;
            return not;
        }

        /// <summary>
        /// Watch for changes of the given property on the given propertySource
        /// </summary>
        /// <param name="propertySource">Object to observe a property on</param>
        /// <param name="property">Property on the object to observe</param>
        /// <param name="notifier">Handler to invoke when the property changes</param>
        /// <returns>The constructed PropertyChangeNotifier</returns>
        public static PropertyChangeNotifier AddValueChanged(DependencyObject propertySource, DependencyProperty property, PropertyChangedCallback notifier)
        {
            return AddValueChanged(propertySource, new PropertyPath(property), notifier);
        }

        /// <summary>
        /// Event raised when the selected property changed
        /// </summary>
        public event PropertyChangedCallback ValueChanged;

        /// <summary>
        /// Initialises a new instance of the <see cref="PropertyChangeNotifier"/> class, using a PropertyPath
        /// </summary>
        /// <param name="propertySource">Object to observe a property on</param>
        /// <param name="property">Property on the object to observe</param>
        public PropertyChangeNotifier(DependencyObject propertySource, PropertyPath property)
        {
            if (propertySource == null)
                throw new ArgumentNullException("propertySource");
            if (property == null)
                throw new ArgumentNullException("property");

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
            var handler = this.ValueChanged;
            if (handler != null)
                handler(this, e);
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
