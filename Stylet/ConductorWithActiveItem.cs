using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public abstract class ConductorBaseWithActiveItem<T> : Screen, IConductor<T>, IParent<T> where T : class
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

        public virtual IEnumerable<T> GetChildren()
        {
            return new[] { ActiveItem };
        }

        protected virtual void ChangeActiveItem(T newItem, bool closePrevious)
        {
            ScreenExtensions.TryDeactivate(this.ActiveItem, closePrevious);

            var newItemAsChild = newItem as IChild;
            if (newItemAsChild != null && newItemAsChild.Parent != this)
                newItemAsChild.Parent = this;

            if (this.IsActive)
            {
                ScreenExtensions.TryActivate(newItem);

                this._activeItem = newItem;
                this.NotifyOfPropertyChange(() => this.ActiveItem);
            }
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

        protected override void OnActivate()
        {
            ScreenExtensions.TryActivate(this.ActiveItem);
        }

        protected override void OnDeactivate(bool close)
        {
            ScreenExtensions.TryDeactivate(this.ActiveItem, close);
        }
    }

}
