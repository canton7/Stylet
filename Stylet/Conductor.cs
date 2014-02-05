using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public abstract class ConductorWithActiveItem<T> : Screen, IConductor<T>, IParent<T> where T : class
    {
        protected bool CloseConductedItemsWhenConductorCannotClose = false;

        private T _activeItem;
        public T ActiveItem
        {
            get { return this._activeItem; }
            set { this.ActivateItem(value); }
        }

        public abstract void ActivateItem(T item);

        public abstract void DeactivateItem(T item, bool close);

        public IEnumerable<T> GetChildren()
        {
            return new[] { ActiveItem };
        }

        protected virtual void ChangeActiveItem(T newItem, bool closePrevious)
        {
            var activeItem = this.ActiveItem as IDeactivate;
            if (activeItem != null)
                activeItem.Deactivate(closePrevious);

            var newItemAsChild = newItem as IChild;
            if (newItemAsChild != null && newItemAsChild.Parent != this)
                newItemAsChild.Parent = this;

            if (this.IsActive)
            {
                this.ActivateItemIfActive(newItem);

                this._activeItem = newItem;
                this.NotifyOfPropertyChange(() => this.ActiveItem);
            }
        }

        protected void ActivateItemIfActive(T item)
        {
            var itemAsIActivate = item as IActivate;
            if (this.IsActive && itemAsIActivate != null)
                itemAsIActivate.Activate();
        }

        protected virtual async Task<IEnumerable<T>> ItemsThatCanCloseAsync(IEnumerable<T> toClose)
        {
            var results = await Task.WhenAll(toClose.Select(x => this.CanCloseItem(x).ContinueWith(t => new { Item = x, Result = t.Result })));
            if (this.CloseConductedItemsWhenConductorCannotClose)
                return results.Where(x => x.Result).Select(x => x.Item);
            else
                return results.All(x => x.Result) ? results.Select(x => x.Item) : Enumerable.Empty<T>();
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

    public class Conductor<T> : ConductorWithActiveItem<T> where T : class
    {
        public override async void ActivateItem(T item)
        {
            if (item != null && item.Equals(this.ActiveItem))
                this.ActivateItemIfActive(item);
            else if (await this.CanCloseItem(this.ActiveItem))
                this.ChangeActiveItem(item, true);
        }

        public override async void DeactivateItem(T item, bool close)
        {
            if (item == null || !item.Equals(this.ActiveItem))
                return;

            if (await this.CanCloseItem(item))
                this.ChangeActiveItem(default(T), close);
        }

        public Task<bool> CanClose()
        {
            return this.CanCloseItem(this.ActiveItem);
        }

        protected override void OnActivate()
        {
            this.ActivateItemIfActive(this.ActiveItem);
        }

        protected override void OnDeactivate(bool close)
        {
            var activeItemAsDeactivate = this.ActiveItem as IDeactivate;
            if (activeItemAsDeactivate != null)
                activeItemAsDeactivate.Deactivate(close);
        }
    }
}
