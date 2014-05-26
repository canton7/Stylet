using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        protected virtual void EnsureItem(T newItem)
        {
            if (newItem == null)
                throw new ArgumentNullException("newItem");

            var newItemAsChild = newItem as IChild;
            if (newItemAsChild != null && newItemAsChild.Parent != this)
                newItemAsChild.Parent = this;
        }

        /// <summary>
        /// Utility method to determine if all of the give items can close
        /// </summary>
        protected virtual async Task<bool> CanAllItemsCloseAsync(IEnumerable<T> toClose)
        {
            var results = await Task.WhenAll(toClose.Select(x => this.CanCloseItem(x)));
            return results.All(x => x);
        }

        /// <summary>
        /// Determine if the given item can be closed
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual Task<bool> CanCloseItem(T item)
        {
            var itemAsGuardClose = item as IGuardClose;
            if (itemAsGuardClose != null)
                return itemAsGuardClose.CanCloseAsync();
            else
                return Task.FromResult(true);
        }

        void IChildDelegate.CloseItem(object item, bool? dialogResult)
        {
            T typedItem = item as T;
            if (typedItem != null)
                this.CloseItem(typedItem);
        }
    }
}
