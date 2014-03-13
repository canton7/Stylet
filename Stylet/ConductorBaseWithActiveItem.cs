using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T> where T : class
    {
        private T _activeItem;
        public T ActiveItem
        {
            get { return this._activeItem; }
            set { this.ActivateItem(value); }
        }

        public override IEnumerable<T> GetChildren()
        {
            return new[] { ActiveItem };
        }

        protected virtual void ChangeActiveItem(T newItem, bool closePrevious)
        {
            ScreenExtensions.TryDeactivate(this.ActiveItem, closePrevious);
            if (closePrevious)
                this.CleanUpAfterClose(this.ActiveItem);

            newItem = this.EnsureItem(newItem);

            if (this.IsActive)
                ScreenExtensions.TryActivate(newItem);

            this._activeItem = newItem;
            this.NotifyOfPropertyChange(() => this.ActiveItem);
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
