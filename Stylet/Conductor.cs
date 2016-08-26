using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Conductor with a single active item, and no other items
    /// </summary>
    /// <typeparam name="T">Type of child to conduct</typeparam>
    public partial class Conductor<T> : ConductorBaseWithActiveItem<T> where T : class
    {
        /// <summary>
        /// Activate the given item, discarding the previous ActiveItem
        /// </summary>
        /// <param name="item">Item to active</param>
        public override async void ActivateItem(T item)
        {
            if (item != null && item.Equals(this.ActiveItem))
            {
                if (this.IsActive)
                    ScreenExtensions.TryActivate(item);
            }
            else if (await this.CanCloseItem(this.ActiveItem)) 
            {
                // CanCloseItem is null-safe

                this.ChangeActiveItem(item, true);
            }
        }

        /// <summary>
        /// Deactive the given item
        /// </summary>
        /// <param name="item">Item to deactivate</param>
        public override void DeactivateItem(T item)
        {
            if (item != null && item.Equals(this.ActiveItem))
                ScreenExtensions.TryDeactivate(this.ActiveItem);
        }

        /// <summary>
        /// Close the given item
        /// </summary>
        /// <param name="item">Item to close</param>
        public override async void CloseItem(T item)
        {
            if (item == null || !item.Equals(this.ActiveItem))
                return;

            if (await this.CanCloseItem(item))
                 this.ChangeActiveItem(default(T), true);
        }

        /// <summary>
        /// Determine if this conductor can close. Depends on whether the ActiveItem can close
        /// </summary>
        /// <returns>Task indicating whether this can be closed</returns>
        public override Task<bool> CanCloseAsync()
        {
            // Temporarily, until we remove CanClose
#pragma warning disable CS0618 // Type or member is obsolete
            if (!this.CanClose())
#pragma warning restore CS0618 // Type or member is obsolete
                return Task.FromResult(false);
            return this.CanCloseItem(this.ActiveItem);
        }
    }
}
