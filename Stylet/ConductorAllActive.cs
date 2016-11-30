using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Stylet
{
    public partial class Conductor<T>
    {
        /// <summary>
        /// Contains specific Conductor{T} collection types
        /// </summary>
        public partial class Collection
        {
            /// <summary>
            /// Conductor which has many items, all of which active at the same time
            /// </summary>
            public class AllActive : ConductorBase<T>
            {
                private readonly BindableCollection<T> items = new BindableCollection<T>();

                private List<T> itemsBeforeReset;

                /// <summary>
                /// Gets all items associated with this conductor
                /// </summary>
                public IObservableCollection<T> Items
                {
                    get { return this.items; }
                }

                /// <summary>
                /// Initialises a new instance of the <see cref="Conductor{T}.Collection.AllActive"/> class
                /// </summary>
                public AllActive()
                {
                    this.items.CollectionChanging += (o, e) =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Reset:
                                this.itemsBeforeReset = this.items.ToList();
                                break;
                        }
                    };

                    this.items.CollectionChanged += (o, e) =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                this.ActivateAndSetParent(e.NewItems);
                                break;

                            case NotifyCollectionChangedAction.Remove:
                                this.CloseAndCleanUp(e.OldItems, this.DisposeChildren);
                                break;

                            case NotifyCollectionChangedAction.Replace:
                                this.ActivateAndSetParent(e.NewItems);
                                this.CloseAndCleanUp(e.OldItems, this.DisposeChildren);
                                break;

                            case NotifyCollectionChangedAction.Reset:
                                this.ActivateAndSetParent(this.items.Except(this.itemsBeforeReset));
                                this.CloseAndCleanUp(this.itemsBeforeReset.Except(this.items), this.DisposeChildren);
                                this.itemsBeforeReset = null;
                                break;
                        }
                    };
                }

                /// <summary>
                /// Active all items in a given collection if appropriate, and set the parent of all items to this
                /// </summary>
                /// <param name="items">Items to manipulate</param>
                protected virtual void ActivateAndSetParent(IEnumerable items)
                {
                    this.SetParentAndSetActive(items, this.IsActive);
                }

                /// <summary>
                /// Activates all items whenever this conductor is activated
                /// </summary>
                protected override void OnActivate()
                {
                    // Copy the list, in case someone tries to modify it as a result of being activated
                    var itemsToActivate = this.items.OfType<IScreenState>().ToList();
                    foreach (var item in itemsToActivate)
                    {
                        item.Activate();
                    }
                }

                /// <summary>
                /// Deactivates all items whenever this conductor is deactivated
                /// </summary>
                protected override void OnDeactivate()
                {
                    // Copy the list, in case someone tries to modify it as a result of being activated
                    var itemsToDeactivate = this.items.OfType<IScreenState>().ToList();
                    foreach (var item in itemsToDeactivate)
                    {
                        item.Deactivate();
                    }
                }

                /// <summary>
                /// Close, and clean up, all items when this conductor is closed
                /// </summary>
                protected override void OnClose()
                {
                    // Copy the list, in case someone tries to modify it as a result of being closed
                    // We've already been deactivated by this point    
                    var itemsToClose = this.items.ToList();
                    foreach (var item in itemsToClose)
                    {
                        this.CloseAndCleanUp(item, this.DisposeChildren);
                    }
                    
                    this.items.Clear();
                }

                /// <summary>
                /// Determine if the conductor can close. Returns true if and when all items can close
                /// </summary>
                /// <returns>A Task indicating whether this conductor can close</returns>
                public override Task<bool> CanCloseAsync()
                {
                    // Temporarily, until we remove CanClose
#pragma warning disable CS0618 // Type or member is obsolete
                    if (!this.CanClose())
#pragma warning restore CS0618 // Type or member is obsolete
                        return Task.FromResult(false);
                    return this.CanAllItemsCloseAsync(this.items);
                }

                /// <summary>
                /// Activate the given item, and add it to the Items collection
                /// </summary>
                /// <param name="item">Item to activate</param>
                public override void ActivateItem(T item)
                {
                    if (item == null)
                        return;

                    this.EnsureItem(item);

                    if (this.IsActive)
                        ScreenExtensions.TryActivate(item);
                    else
                        ScreenExtensions.TryDeactivate(item);
                }

                /// <summary>
                /// Deactive the given item
                /// </summary>
                /// <param name="item">Item to deactivate</param>
                public override void DeactivateItem(T item)
                {
                    ScreenExtensions.TryDeactivate(item);
                }

                /// <summary>
                /// Close a particular item, removing it from the Items collection
                /// </summary>
                /// <param name="item">Item to close</param>
                public async override void CloseItem(T item)
                {
                    if (item == null)
                        return;

                    if (await this.CanCloseItem(item))
                    {
                        this.CloseAndCleanUp(item, this.DisposeChildren);
                        this.items.Remove(item);
                    }
                }

                /// <summary>
                /// Returns all children of this parent
                /// </summary>
                /// <returns>All children associated with this conductor</returns>
                public override IEnumerable<T> GetChildren()
                {
                    return this.items;
                }

                /// <summary>
                /// Ensure an item is ready to be activated, by adding it to the items collection, as well as setting it up
                /// </summary>
                /// <param name="newItem">Item to ensure</param>
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
