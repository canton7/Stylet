using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Base class for all conductors
    /// </summary>
    /// <typeparam name="T">Type of item to be conducted</typeparam>
    public abstract class ConductorBase<T> : Screen, IConductor<T>, IParent<T>, IChildDelegate where T : class
    {
        /// <summary>
        /// Gets or sets a value indicating whether to dispose a child when it's closed. True by default
        /// </summary>
        public virtual bool DisposeChildren { get; set; }

        /// <summary>
        /// Retrieves the Item or Items associated with this Conductor
        /// </summary>
        /// <returns>Item or Items associated with this Conductor</returns>
        public abstract IEnumerable<T> GetChildren();

        /// <summary>
        /// Activate the given item
        /// </summary>
        /// <param name="item">Item to activate</param>
        public abstract void ActivateItem(T item);

        /// <summary>
        /// Deactivate the given item
        /// </summary>
        /// <param name="item">Item to deactivate</param>
        public abstract void DeactivateItem(T item);

        /// <summary>
        /// Close the given item
        /// </summary>
        /// <param name="item">Item to deactivate</param>
        public abstract void CloseItem(T item);

        /// <summary>
        /// Ensure an item is ready to be activated
        /// </summary>
        /// <param name="newItem">Item to use</param>
        protected virtual void EnsureItem(T newItem)
        {
            Debug.Assert(newItem != null);

            var newItemAsChild = newItem as IChild;
            if (newItemAsChild != null && newItemAsChild.Parent != this)
                newItemAsChild.Parent = this;
        }

        /// <summary>
        /// Utility method to determine if all of the give items can close
        /// </summary>
        /// <param name="itemsToClose">Items to close</param>
        /// <returns>Task indicating whether all items can close</returns>
        protected virtual async Task<bool> CanAllItemsCloseAsync(IEnumerable<T> itemsToClose)
        {
            var results = await Task.WhenAll(itemsToClose.Select(this.CanCloseItem));
            return results.All(x => x);
        }

        /// <summary>
        /// Determine if the given item can be closed
        /// </summary>
        /// <param name="item">Item to use</param>
        /// <returns>Task indicating whether the item can be closed</returns>
        protected virtual Task<bool> CanCloseItem(T item)
        {
            var itemAsGuardClose = item as IGuardClose;
            if (itemAsGuardClose != null)
                return itemAsGuardClose.CanCloseAsync();
            else
                return Task.FromResult(true);
        }

        /// <summary>
        /// Close the given child
        /// </summary>
        /// <param name="item">Child to close</param>
        /// <param name="dialogResult">Unused in this scenario</param>
        void IChildDelegate.CloseItem(object item, bool? dialogResult)
        {
            T typedItem = item as T;
            if (typedItem != null)
                this.CloseItem(typedItem);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ConductorBase{T}"/> class
        /// </summary>
        public ConductorBase()
        {
            this.DisposeChildren = true;
        }
    }
}
