using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public class PropertyChangedBase : INotifyPropertyChanged, INotifyPropertyChangedDispatcher
    {
        private Action<Action> _propertyChangedDispatcher = Execute.DefaultPropertyChangedDispatcher;
        public Action<Action> PropertyChangedDispatcher
        {
            get { return this._propertyChangedDispatcher; }
            set { this._propertyChangedDispatcher = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Refresh()
        {
            this.NotifyOfPropertyChange(String.Empty);
        }

        protected void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            this.NotifyOfPropertyChange(property.NameForProperty());
        }

        protected virtual void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                this.PropertyChangedDispatcher(() => handler(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        protected virtual void SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Comparer<T>.Default.Compare(field, value) != 0)
            {
                field = value;
                this.NotifyOfPropertyChange(propertyName);
            }
        }
    }
}
