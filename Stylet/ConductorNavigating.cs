using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stylet
{
    public partial class Conductor<T>
    {
        /// <summary>
        /// Stack-based navigation. A Conductor which has one active item, and a stack of previous items
        /// </summary>
        public class StackNavigation : ConductorBaseWithActiveItem<T>
        {
            // We need to remove arbitrary items, so no Stack<T> here!
            private readonly List<T> history = new List<T>();

            /// <summary>
            /// Activate the given item. This deactivates the previous item, and pushes it onto the history stack
            /// </summary>
            /// <param name="item">Item to activate</param>
            public override void ActivateItem(T item)
            {
                if (item != null && item.Equals(this.ActiveItem))
                {
                    if (this.IsActive)
                        ScreenExtensions.TryActivate(this.ActiveItem);
                }
                else
                {
                    if (this.ActiveItem != null)
                        this.history.Add(this.ActiveItem);
                    this.ChangeActiveItem(item, false);
                }
            }

            /// <summary>
            /// Deactivate the given item
            /// </summary>
            /// <param name="item">Item to deactivate</param>
            public override void DeactivateItem(T item)
            {
                ScreenExtensions.TryDeactivate(item);
            }

            /// <summary>
            /// Close the active item, and re-activate the top item in the history stack
            /// </summary>
            public void GoBack()
            {
                this.CloseItem(this.ActiveItem);
            }

            /// <summary>
            /// Close and remove all items in the history stack, leaving the ActiveItem
            /// </summary>
            public void Clear()
            {
                foreach (var item in this.history)
                {
                    this.CloseAndCleanUp(item, this.DisposeChildren);
                }
                this.history.Clear();
            }

            /// <summary>
            /// Close the given item. If it was the ActiveItem, activate the top item in the history stack
            /// </summary>
            /// <param name="item">Item to close</param>
            public override async void CloseItem(T item)
            {
                if (item == null || !await this.CanCloseItem(item))
                    return;

                if (item.Equals(this.ActiveItem))
                {
                    var newItem = default(T);
                    if (this.history.Count > 0)
                    {
                        newItem = this.history.Last();
                        this.history.RemoveAt(this.history.Count - 1);
                    }
                    this.ChangeActiveItem(newItem, true);
                }
                else if (this.history.Contains(item))
                {
                    this.CloseAndCleanUp(item, this.DisposeChildren);
                    this.history.Remove(item);
                }
            }

            /// <summary>
            /// Returns true if and when all items (ActiveItem + everything in the history stack) can close
            /// </summary>
            /// <returns>A task indicating whether this conductor can close</returns>
            public override Task<bool> CanCloseAsync()
            {
                // Temporarily, until we remove CanClose
#pragma warning disable CS0618 // Type or member is obsolete
                if (!this.CanClose())
#pragma warning restore CS0618 // Type or member is obsolete
                    return Task.FromResult(false);
                return this.CanAllItemsCloseAsync(this.history.Concat(new[] { this.ActiveItem }));
            }

            /// <summary>
            /// Ensures that all children are closed when this conductor is closed
            /// </summary>
            protected override void OnClose()
            {
                // We've already been deactivated by this point
                foreach (var item in this.history)
                    this.CloseAndCleanUp(item, this.DisposeChildren);
                this.history.Clear();

                this.CloseAndCleanUp(this.ActiveItem, this.DisposeChildren);
            }
        }
    }
}
