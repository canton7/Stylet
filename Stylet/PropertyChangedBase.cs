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
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Refresh()
        {
            this.NotifyOfPropertyChange(String.Empty);
        }

        protected void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            string propertyName;
            if (property.Body is UnaryExpression)
                propertyName = ((MemberExpression)((UnaryExpression)property.Body).Operand).Member.Name;
            else
                propertyName = ((MemberExpression)property.Body).Member.Name;
            this.NotifyOfPropertyChange(propertyName);
        }

        protected virtual void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
        {
            Execute.OnUIThread(() =>
            {
                var handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            });
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
