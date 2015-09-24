using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Stylet
{
    /// <summary>
    /// Represents a collection which is observasble
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections</typeparam>
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
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    public interface IReadOnlyObservableCollection<out T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyCollectionChanging
    { }

    /// <summary>
    /// ObservableCollection subclass which supports AddRange and RemoveRange
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    public class BindableCollection<T> : ObservableCollection<T>, IObservableCollection<T>, IReadOnlyObservableCollection<T>
    {
        /// <summary>
        ///  We have to disable notifications when adding individual elements in the AddRange and RemoveRange implementations
        /// </summary>
        private bool isNotifying = true;

        /// <summary>
        /// Initialises a new instance of the <see cref="BindableCollection{T}"/> class
        /// </summary>
        public BindableCollection()
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="BindableCollection{T}"/> class that contains the given members
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied</param>
        public BindableCollection(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// Occurs when the collection will change
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanging;

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
        /// Raises the CollectionChanging event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected virtual void OnCollectionChanging(NotifyCollectionChangedEventArgs e)
        {
            if (this.isNotifying)
            {
                var handler = this.CollectionChanging;
                if (handler != null)
                {
                    using (this.BlockReentrancy())
                    {
                        handler(this, e);
                    }
                }
            }
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
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                var previousNotificationSetting = this.isNotifying;
                this.isNotifying = false;
                var index = this.Count;
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
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                var previousNotificationSetting = this.isNotifying;
                this.isNotifying = false;
                foreach (var item in items)
                {
                    var index = this.IndexOf(item);
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
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is added to list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        /// <param name="index">Index at which to insert the item</param>
        /// <param name="item">Item to insert</param>
        protected override void InsertItem(int index, T item)
        {
            Execute.OnUIThreadSync(() =>
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                base.InsertItem(index, item);
            });
        }

        /// <summary>
        /// Called by base class Collection{T} when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        /// <param name="index">Index of the item to set</param>
        /// <param name="item">Item to set</param>
        protected override void SetItem(int index, T item)
        {
            Execute.OnUIThreadSync(() =>
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, this[index], index));
                base.SetItem(index, item);
            });
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is removed from list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        /// <param name="index">Index of the item to remove</param>
        protected override void RemoveItem(int index)
        {
            Execute.OnUIThreadSync(() =>
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this[index], index));
                base.RemoveItem(index);
            });
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            Execute.OnUIThreadSync(() =>
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                base.ClearItems();
            });
        }
    }
}
