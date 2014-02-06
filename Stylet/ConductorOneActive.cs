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
            public class OneActive : ConductorBaseWithActiveItem<T>
            {
                private BindableCollection<T> items = new BindableCollection<T>();
                public IObservableCollection<T> Items
                {
                    get { return this.items; }
                }

                public OneActive()
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

                public override IEnumerable<T> GetChildren()
                {
                    return this.items;
                }

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

                public override async void DeactivateItem(T item, bool close)
                {
                    if (item == null)
                        return;

                    if (close)
                    {
                        if (await this.CanCloseItem(item))
                            this.CloseItem(item);
                    }
                    else
                    {
                        ScreenExtensions.TryDeactivate(item, false);
                    }
                }

                private void CloseItem(T item)
                {
                    if (item.Equals(this.ActiveItem))
                    {
                        var index = this.items.IndexOf(item);
                        var nextItem = this.DetermineNextItemToActivate(this.items, index);
                        this.ChangeActiveItem(nextItem, true);
                    }
                    else
                    {
                        ScreenExtensions.TryDeactivate(item, true);
                    }

                    this.items.Remove(item);
                }

                protected virtual T DetermineNextItemToActivate(IList<T> list, int indexOfItemBeingRemoved)
                {
                    if (list.Count > 1)
                    {
                        if (indexOfItemBeingRemoved == 0)
                            return list[1];
                        else
                            return list[indexOfItemBeingRemoved - 1];
                    }
                    else
                    {
                        return default(T);
                    }
                }

                public override Task<bool> CanCloseAsync()
                {
                    return this.CanAllItemsCloseAsync(this.items);
                }

                protected override void OnDeactivate(bool close)
                {
                    if (close)
                    {
                        foreach (var item in this.items.OfType<IDeactivate>())
                            item.Deactivate(true);
                    }
                    else
                    {
                        base.OnDeactivate(false);
                    }
                }

                protected override T EnsureItem(T newItem)
                {
                    if (newItem == null)
                    {
                        newItem = this.DetermineNextItemToActivate(this.items, this.ActiveItem == null ? 0 : this.items.IndexOf(this.ActiveItem));
                    }
                    else
                    {
                        if (!this.items.Contains(newItem))
                            this.items.Add(newItem);
                    }
                    return base.EnsureItem(newItem);

                }
            }
        }
    }
}
