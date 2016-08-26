using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Stylet
{
    /// <summary>
    /// Base class for things which can raise PropertyChanged events
    /// </summary>
    public abstract class PropertyChangedBase : INotifyPropertyChanged, INotifyPropertyChangedDispatcher
    {
        private Action<Action> _propertyChangedDispatcher = Execute.DefaultPropertyChangedDispatcher;

        /// <summary>
        /// Gets or sets the dispatcher to use to dispatch PropertyChanged events. Defaults to Execute.DefaultPropertyChangedDispatcher
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public virtual Action<Action> PropertyChangedDispatcher
        {
            get { return this._propertyChangedDispatcher; }
            set { this._propertyChangedDispatcher = value; }
        }

        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Refresh all properties
        /// </summary>
        public void Refresh()
        {
            this.NotifyOfPropertyChange(String.Empty);
        }

        /// <summary>
        /// Raise a PropertyChanged notification from the property in the given expression, e.g. NotifyOfPropertyChange(() => this.Property)
        /// </summary>
        /// <typeparam name="TProperty">Type of property being notified</typeparam>
        /// <param name="property">Expression describing the property to raise a PropertyChanged notification for</param>
        protected virtual void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            this.OnPropertyChanged(property.NameForProperty());
        }

        /// <summary>
        /// Raise a PropertyChanged notification from the property with the given name
        /// </summary>
        /// <param name="propertyName">Name of the property to raise a PropertyChanged notification for. Defaults to the calling property</param>
        protected virtual void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
        {
            this.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Fires the PropertyChanged notification.
        /// </summary>
        /// <remarks>Specially named so that Fody.PropertyChanged calls it</remarks>
        /// <param name="propertyName">Name of the property to raise the notification for</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChangedDispatcher(() => 
                {
                    var handler = this.PropertyChanged;
                    if (handler != null)
                        handler(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }

        /// <summary>
        /// Takes, by reference, a field, and its new value. If field != value, will set field = value and raise a PropertyChanged notification
        /// </summary>
        /// <typeparam name="T">Type of field being set and notified</typeparam>
        /// <param name="field">Field to assign</param>
        /// <param name="value">Value to assign to the field, if it differs</param>
        /// <param name="propertyName">Name of the property to notify for. Defaults to the calling property</param>
        /// <returns>True if field != value and a notification was raised; false otherwise</returns>
        protected virtual bool SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                this.NotifyOfPropertyChange(propertyName: propertyName);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
