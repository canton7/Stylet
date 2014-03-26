using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public partial class Conductor<T>
    {
        public partial class Collections
        {
            /// <summary>
            /// Conductor which has many items, all of which active at the same time
            /// </summary>
            public class AllActive : ConductorBase<T>
            {
                private BindableCollection<T> items = new BindableCollection<T>();
                public IObservableCollection<T> Items
                {
                    get { return this.items; }
                }

                public AllActive()
                {
                    this.items.CollectionChanged += (o, e) =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                this.SetParent(e.NewItems, this);
                                break;

                            case NotifyCollectionChangedAction.Remove:
                                this.SetParent(e.OldItems, null);
                                break;

                            case NotifyCollectionChangedAction.Replace:
                                this.SetParent(e.NewItems, this);
                                this.SetParent(e.OldItems, null);
                                break;

                            case NotifyCollectionChangedAction.Reset:
                                this.SetParent(this.items, this);
                                break;
                        }
                    };
                }

                private void SetParent(IEnumerable items, object parent)
                {
                    foreach (var child in items.OfType<IChild>())
                    {
                        child.Parent = parent;
                    }
                }

                protected override void OnActivate()
                {
                    foreach (var item in this.items.OfType<IActivate>())
                    {
                        item.Activate();
                    }
                }

                protected override void OnDeactivate()
                {
                    foreach (var item in this.items.OfType<IDeactivate>())
                    {
                        item.Deactivate();
                    }
                }

                protected override void OnClose()
                {
                    // We've already been deactivated by this point    
                    items.Clear();
                }

                public override Task<bool> CanCloseAsync()
                {
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

                    item = this.EnsureItem(item);

                    if (this.IsActive)
                        ScreenExtensions.TryActivate(item);
                }

                /// <summary>
                /// Deactive the given item
                /// </summary>
                /// <param name="item">Item to deactivate</param>
                public override async void DeactivateItem(T item)
                {
                    ScreenExtensions.TryDeactivate(item);
                }

                public async override void CloseItem(T item)
                {
                    if (item == null)
                        return;

                    if (await this.CanCloseItem(item))
                    {
                        ScreenExtensions.TryClose(item);
                        this.items.Remove(item);
                    }
                }

                public override IEnumerable<T> GetChildren()
                {
                    return this.items;
                }

                protected override T EnsureItem(T newItem)
                {
                    if (!this.items.Contains(newItem))
                        this.items.Add(newItem);

                    return base.EnsureItem(newItem);
                }
            }
        }
    }
}
