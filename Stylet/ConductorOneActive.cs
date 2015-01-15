using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Stylet
{
    public partial class Conductor<T>
    {
        public partial class Collection
        {
            /// <summary>
            /// Conductor with many items, only one of which is active
            /// </summary>
            public class OneActive : ConductorBaseWithActiveItem<T>
            {
                private readonly BindableCollection<T> items = new BindableCollection<T>();

                /// <summary>
                /// Gets the tems owned by this Conductor, one of which is active
                /// </summary>
                public IObservableCollection<T> Items
                {
                    get { return this.items; }
                }

                /// <summary>
                /// Initialises a new instance of the <see cref="Conductor{T}.Collection.OneActive"/> class
                /// </summary>
                public OneActive()
                {
                    this.items.CollectionChanged += (o, e) =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                this.SetParent(e.NewItems);
                                break;

                            case NotifyCollectionChangedAction.Remove:
                                this.CloseAndCleanUp(e.OldItems, this.DisposeChildren);
                                this.ActiveItemMayHaveBeenRemovedFromItems();
                                break;

                            case NotifyCollectionChangedAction.Replace:
                                this.SetParent(e.NewItems);
                                this.CloseAndCleanUp(e.OldItems, this.DisposeChildren);
                                this.ActiveItemMayHaveBeenRemovedFromItems();
                                break;

                            case NotifyCollectionChangedAction.Reset:
                                this.SetParent(this.items);
                                this.ActiveItemMayHaveBeenRemovedFromItems();
                                break;
                        }
                    };
                }

                /// <summary>
                /// Called when the ActiveItem may have been removed from the Items collection. If it has, will change the ActiveItem to something sensible
                /// </summary>
                protected virtual void ActiveItemMayHaveBeenRemovedFromItems()
                {
                    if (this.items.Contains(this.ActiveItem))
                        return;

                    this.ChangeActiveItem(this.items.FirstOrDefault(), true);
                }

                /// <summary>
                /// Return all items associated with this conductor
                /// </summary>
                /// <returns>All children associated with this conductor</returns>
                public override IEnumerable<T> GetChildren()
                {
                    return this.items;
                }

                /// <summary>
                /// Activate the given item and set it as the ActiveItem, deactivating the previous ActiveItem
                /// </summary>
                /// <param name="item">Item to deactivate</param>
                public override void ActivateItem(T item)
                {
                    if (item != null && item.Equals(this.ActiveItem))
                    {
                        if (this.IsActive)
                            ScreenExtensions.TryActivate(this.ActiveItem);
                    }
                    else
                    {
                        this.ChangeActiveItem(item, false);
                    }
                }

                /// <summary>
                /// Deactive the given item, and choose another item to set as the ActiveItem
                /// </summary>
                /// <param name="item">Item to deactivate</param>
                public override void DeactivateItem(T item)
                {
                    if (item == null)
                        return;

                    if (item.Equals(this.ActiveItem))
                    {
                        var nextItem = this.DetermineNextItemToActivate(item);
                        this.ChangeActiveItem(nextItem, false);
                    }
                    else
                    {
                        ScreenExtensions.TryDeactivate(item);
                    }
                }

                /// <summary>
                /// Close the given item (if and when possible, depending on IGuardClose.CanCloseAsync). This will deactive if it is the active item
                /// </summary>
                /// <param name="item">Item to close</param>
                public override async void CloseItem(T item)
                {
                    if (item == null || !await this.CanCloseItem(item))
                        return;

                    if (item.Equals(this.ActiveItem))
                    {
                        var nextItem = this.DetermineNextItemToActivate(item);
                        this.ChangeActiveItem(nextItem, true);
                    }
                    else
                    {
                        this.CloseAndCleanUp(item, this.DisposeChildren);
                    }

                    this.items.Remove(item);
                }

                /// <summary>
                /// Given a list of items, and and item which is going to be removed, choose a new item to be the next ActiveItem 
                /// </summary>
                /// <param name="itemToRemove">Item to remove</param>
                /// <returns>The next item to activate, or default(T) if no such item exists</returns>
                protected virtual T DetermineNextItemToActivate(T itemToRemove)
                {
                    if (itemToRemove == null)
                    {
                        return this.items.FirstOrDefault();
                    }
                    else if (this.items.Count > 1)
                    {
                        // indexOfItemBeingRemoved *can* be -1 - if the item being removed doesn't exist in the list
                        var indexOfItemBeingRemoved = this.items.IndexOf(itemToRemove);

                        if (indexOfItemBeingRemoved < 0)
                            return this.items[0];
                        else if (indexOfItemBeingRemoved == 0)
                            return this.items[1];
                        else
                            return this.items[indexOfItemBeingRemoved - 1];
                    }
                    else
                    {
                        return default(T);
                    }
                }

                /// <summary>
                /// Returns true if and when all children can close
                /// </summary>
                /// <returns>A task indicating whether this conductor can close</returns>
                public override Task<bool> CanCloseAsync()
                {
                    return this.CanAllItemsCloseAsync(this.items);
                }

                /// <summary>
                /// Ensures that all items are closed when this conductor is closed
                /// </summary>
                protected override void OnClose()
                {
                    // We've already been deactivated by this point
                    foreach (var item in this.items)
                        this.CloseAndCleanUp(item, this.DisposeChildren);
                    this.items.Clear();
                }

                /// <summary>
                /// Ensure an item is ready to be activated
                /// </summary>
                /// <param name="newItem">New item to ensure</param>
                protected override void EnsureItem(T newItem)
                {
                    if (!this.items.Contains(newItem))
                        this.items.Add(newItem);

                    base.EnsureItem(newItem);
                }
            }
        }
    }
}
