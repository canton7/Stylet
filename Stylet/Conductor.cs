using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Conductor with a single active item, and no other items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class Conductor<T> : ConductorBaseWithActiveItem<T> where T : class
    {
        /// <summary>
        /// Activate the given item, discarding the previous ActiveItem
        /// </summary>
        /// <param name="item"></param>
        public override async void ActivateItem(T item)
        {
            if (item != null && item.Equals(this.ActiveItem))
            {
                if (this.IsActive)
                    ScreenExtensions.TryActivate(item);
            }
            else if (await this.CanCloseItem(this.ActiveItem))
            {
                this.ChangeActiveItem(item, true);
            }
        }

        /// <summary>
        /// Deactive the given item, optionally closing it as well
        /// </summary>
        /// <param name="item">Item to deactivate</param>
        /// <param name="close">True to also close the item</param>
        public override async void DeactivateItem(T item, bool close)
        {
            if (item == null || !item.Equals(this.ActiveItem))
                return;

            if (close)
            {
                if (await this.CanCloseItem(item))
                    this.ChangeActiveItem(default(T), close);
            }
            else
            {
                ScreenExtensions.TryDeactivate(this.ActiveItem, false);
            }
        }

        public override Task<bool> CanCloseAsync()
        {
            return this.CanCloseItem(this.ActiveItem);
        }
    }
}
