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
    /// <summary>
    /// Represents a collection which is observasble
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObservableCollection<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <summary>
        /// Add a range of items
        /// </summary>
        /// <param name="items">Items to add</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Remove a range of items
        /// </summary>
        /// <param name="items">Items to remove</param>
        void RemoveRange(IEnumerable<T> items);
    }

    /// <summary>
    /// ObservableCollection subclass which supports AddRange and RemoveRange
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableCollection<T> : ObservableCollection<T>, IObservableCollection<T>
    {
        /// <summary>
        ///  We have to disable notifications when adding individual elements in the AddRange and RemoveRange implementations
        /// </summary>
        private bool isNotifying = true;

        public BindableCollection() : base() { }
        public BindableCollection(IEnumerable<T> collection) : base(collection) { }

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

        /// <summary>
        /// Add a range of items
        /// </summary>
        /// <param name="items">Items to add</param>
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

        /// <summary>
        /// Remove a range of items
        /// </summary>
        /// <param name="items">Items to remove</param>
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

        /// <summary>
        /// Raise a change notification indicating that all bindings should be refreshed
        /// </summary>
        public void Refresh()
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
