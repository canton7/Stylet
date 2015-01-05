using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

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
    /// Interface encapsulating IReadOnlyList and INotifyCollectionChanged
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyObservableCollection<T> : IReadOnlyList<T>, INotifyCollectionChanged
    {
    }

    /// <summary>
    /// ObservableCollection subclass which supports AddRange and RemoveRange
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableCollection<T> : ObservableCollection<T>, IObservableCollection<T>, IReadOnlyObservableCollection<T>
    {
        /// <summary>
        ///  We have to disable notifications when adding individual elements in the AddRange and RemoveRange implementations
        /// </summary>
        private bool isNotifying = true;

        /// <summary>
        /// Create a new empty BindableCollection
        /// </summary>
        public BindableCollection() : base() { }

        /// <summary>
        /// Create a new BindableCollection with the given members
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied</param>
        public BindableCollection(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// Raises the System.Collections.ObjectModel.ObservableCollection{T}.PropertyChanged event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            // Avoid doing a dispatch if nothing's subscribed....
            if (this.isNotifying)
                base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Raises the System.Collections.ObjectModel.ObservableCollection{T}.CollectionChanged event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
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
            Execute.OnUIThreadSync(() =>
            {
                var previousNotificationSetting = this.isNotifying;
                this.isNotifying = false;
                var index = Count;
                foreach (var item in items)
                {
                    base.InsertItem(index, item);
                    index++;
                }
                this.isNotifying = previousNotificationSetting;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                // Can't add with a range, or it throws an exception
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        /// <summary>
        /// Remove a range of items
        /// </summary>
        /// <param name="items">Items to remove</param>
        public virtual void RemoveRange(IEnumerable<T> items)
        {
            Execute.OnUIThreadSync(() =>
            {
                var previousNotificationSetting = this.isNotifying;
                this.isNotifying = false;
                foreach (var item in items)
                {
                    var index = IndexOf(item);
                    if (index >= 0)
                        base.RemoveItem(index);
                }
                this.isNotifying = previousNotificationSetting;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                // Can't remove with a range, or it throws an exception
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        /// <summary>
        /// Raise a change notification indicating that all bindings should be refreshed
        /// </summary>
        public void Refresh()
        {
            Execute.OnUIThreadSync(() =>
            {
                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is added to list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void InsertItem(int index, T item)
        {
            Execute.OnUIThreadSync(() => base.InsertItem(index, item));
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            Execute.OnUIThreadSync(() => base.SetItem(index, item));
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is removed from list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            Execute.OnUIThreadSync(() => base.RemoveItem(index));
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            Execute.OnUIThreadSync(() => base.ClearItems());
        }
    }
}
