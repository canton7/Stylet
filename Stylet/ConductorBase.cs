using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public abstract class ConductorBase<T> : Screen, IConductor<T>, IParent<T> where T : class
    {
        public abstract IEnumerable<T> GetChildren();
        public abstract void ActivateItem(T item);
        public abstract void DeactivateItem(T item, bool close);

        // Ensure an item is ready to be activated
        protected virtual T EnsureItem(T newItem)
        {
            var newItemAsChild = newItem as IChild;
            if (newItemAsChild != null && newItemAsChild.Parent != this)
                newItemAsChild.Parent = this;

            return newItem;
        }

        protected virtual void CleanUpAfterClose(T item)
        {
            var itemAsChild = item as IChild;
            if (itemAsChild != null && itemAsChild.Parent == this)
                itemAsChild.Parent = null;
        }

        protected virtual async Task<bool> CanAllItemsCloseAsync(IEnumerable<T> toClose)
        {
            var results = await Task.WhenAll(toClose.Select(x => this.CanCloseItem(x)));
            return results.All(x => x);
        }

        protected virtual Task<bool> CanCloseItem(T item)
        {
            var itemAsGuardClose = item as IGuardClose;
            if (itemAsGuardClose != null)
                return itemAsGuardClose.CanCloseAsync();
            else
                return Task.FromResult(true);
        }
    }
}
