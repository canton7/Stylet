using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet.SlightlyInternal;

namespace Stylet
{
    public partial class Conductor<T>
    {
        public partial class Collections
        {
            /// <summary>
            /// Conductor which has one active item, and a stack of previous items
            /// </summary>
            public class Navigation : ConductorBaseWithActiveItem<T>
            {
                private List<T> history = new List<T>();

                public override void ActivateItem(T item)
                {
                    if (item != null && item.Equals(this.ActiveItem))
                    {
                        if (this.IsActive)
                            ScreenExtensions.TryActivate(this.ActiveItem);
                    }
                    else
                    {
                        if (this.ActiveItem != null)
                            this.history.Add(this.ActiveItem);
                        this.ChangeActiveItem(item, false);
                    }
                }

                public override void DeactivateItem(T item)
                {
                    ScreenExtensions.TryDeactivate(item);
                }

                public void GoBack()
                {
                    this.CloseItem(this.ActiveItem);
                }

                public void Clear()
                {
                    foreach (var item in this.history)
                        this.CloseAndCleanUp(item);
                    this.history.Clear();
                }

                public override async void CloseItem(T item)
                {
                    if (item == null || !await this.CanCloseItem(item))
                        return;

                    if (item.Equals(this.ActiveItem))
                    {
                        var newItem = default(T);
                        if (this.history.Count > 0)
                        {
                            newItem = this.history.Last();
                            this.history.RemoveAt(this.history.Count-1);
                        }
                        this.ChangeActiveItem(newItem, true);
                    }
                    else if (this.history.Contains(item))
                    {
                        this.CloseAndCleanUp(item);
                        this.history.Remove(item);
                    }
                }

                public override Task<bool> CanCloseAsync()
                {
                    return this.CanAllItemsCloseAsync(this.history.Concat(new[] { this.ActiveItem })); 
                }

                protected override void OnClose()
                {
                    // We've already been deactivated by this point
                    foreach (var item in this.history)
                        this.CloseAndCleanUp(item);
                    this.history.Clear();
                }
            }
        }
    }
}
