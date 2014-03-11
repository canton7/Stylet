using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public interface IObservableCollection<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        void AddRange(IEnumerable<T> items);
        void RemoveRange(IEnumerable<T> items);
    }

    public class BindableCollection<T> : ObservableCollection<T>, IObservableCollection<T>
    {
        private bool isNotifying = true;

        public BindableCollection() : base() { }
        public BindableCollection(IEnumerable<T> collection) : base(collection) { }

        protected void NotifyOfPropertyChange([CallerMemberName] string propertyName = "")
        {
            if (this.isNotifying)
                this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.isNotifying)
                base.OnPropertyChanged(e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.isNotifying)
                base.OnCollectionChanged(e);
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
           var previousNotificationSetting = this.isNotifying;
            this.isNotifying = false;
            var index = Count;
            foreach (var item in items)
            {
                this.InsertItem(index, item);
                index++;
            }
            this.isNotifying = previousNotificationSetting;
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList()));
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            var previousNotificationSetting = this.isNotifying;
            this.isNotifying = false;
            foreach (var item in items)
            {
                var index = IndexOf(item);
                if (index >= 0)
                {
                    this.RemoveItem(index);
                }
            }
            this.isNotifying = previousNotificationSetting;
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items.ToList()));
        }

        public void Refresh()
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
