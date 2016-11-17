using System.Collections.Generic;

namespace Stylet
{
    /// <summary>
    /// Base class for all conductors which had a single active item
    /// </summary>
    /// <typeparam name="T">Type of item being conducted</typeparam>
    public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T>, IHaveActiveItem<T> where T : class
    {
        private T _activeItem;

        /// <summary>
        /// Gets or sets the item which is currently active
        /// </summary>
        public T ActiveItem
        {
            get { return this._activeItem; }
            set { this.ActivateItem(value); }
        }

        /// <summary>
        /// From IParent, fetch all items
        /// </summary>
        /// <returns>Children of this conductor</returns>
        public override IEnumerable<T> GetChildren()
        {
            return new[] { this.ActiveItem };
        }

        /// <summary>
        /// Switch the active item to the given item
        /// </summary>
        /// <param name="newItem">New item to activate</param>
        /// <param name="closePrevious">Whether the previously-active item should be closed</param>
        protected virtual void ChangeActiveItem(T newItem, bool closePrevious)
        {
            ScreenExtensions.TryDeactivate(this.ActiveItem);
            if (closePrevious)
                this.CloseAndCleanUp(this.ActiveItem, this.DisposeChildren);

            if (newItem != null)
            {
                this.EnsureItem(newItem);

                if (this.IsActive)
                    ScreenExtensions.TryActivate(newItem);
                else
                    ScreenExtensions.TryDeactivate(newItem);
            }

            this._activeItem = newItem;
            this.NotifyOfPropertyChange("ActiveItem");
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
            this.CloseAndCleanUp(this.ActiveItem, this.DisposeChildren);
        }
    }
}
