using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet.SlightlyInternal;

namespace Stylet
{
    /// <summary>
    /// Base class for all conductors which had a single active item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T>, IHaveActiveItem<T> where T : class
    {
        private T _activeItem;

        /// <summary>
        /// Item which is currently active
        /// </summary>
        public T ActiveItem
        {
            get { return this._activeItem; }
            set { this.ActivateItem(value); }
        }

        /// <summary>
        /// From IParent, fetch all items
        /// </summary>
        public override IEnumerable<T> GetChildren()
        {
            return new[] { ActiveItem };
        }

        /// <summary>
        /// Switch the active item to the given item
        /// </summary>
        protected virtual void ChangeActiveItem(T newItem, bool closePrevious)
        {
            ScreenExtensions.TryDeactivate(this.ActiveItem);
            if (closePrevious)
                this.CloseAndCleanUp(this.ActiveItem);

            newItem = this.EnsureItem(newItem);

            if (this.IsActive)
                ScreenExtensions.TryActivate(newItem);

            this._activeItem = newItem;
            this.NotifyOfPropertyChange(() => this.ActiveItem);
        }

        /// <summary>
        /// When we're activated, also activate the ActiveItem
        /// </summary>
        protected override void OnActivate()
        {
            ScreenExtensions.TryActivate(this.ActiveItem);
        }

        /// <summary>
        /// When we're deactivated, also deactivate the ActiveItem
        /// </summary>
        protected override void OnDeactivate()
        {
            ScreenExtensions.TryDeactivate(this.ActiveItem);
        }

        /// <summary>
        /// When we're closed, also close the ActiveItem
        /// </summary>
        protected override void OnClose()
        {
            this.CloseAndCleanUp(this.ActiveItem);
        }
    }

}
