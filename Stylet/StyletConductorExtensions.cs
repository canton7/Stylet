using System.Collections;
using System.Linq;

namespace Stylet
{
    // Don't name ConductorExtensions, otherwise it's too obvious when someone types 'Conductor'

    /// <summary>
    /// Extension methods used by the Conductor classes
    /// </summary>
    public static class StyletConductorExtensions
    {
        /// <summary>
        /// For each item in a list, set the parent to the current conductor
        /// </summary>
        /// <typeparam name="T">Type of conductor</typeparam>
        /// <param name="parent">Parent to set the items' parent to</param>
        /// <param name="items">Items to manipulate</param>
        /// <param name="active">True to active the item, false to deactive it</param>
        public static void SetParentAndSetActive<T>(this IConductor<T> parent, IEnumerable items, bool active)
        {
            foreach (var item in items)
            {
                var itemAsChild = item as IChild;
                if (itemAsChild != null)
                    itemAsChild.Parent = parent;

                if (active)
                    ScreenExtensions.TryActivate(item);
                else
                    ScreenExtensions.TryDeactivate(item);
            }
        }

        /// <summary>
        /// Close an item, and clear its parent if it's set to the current parent
        /// </summary>
        /// <typeparam name="T">Type of conductor</typeparam>
        /// <param name="parent">Parent</param>
        /// <param name="item">Item to close and clean up</param>
        /// <param name="dispose">True to dispose children as well as close them</param>
        public static void CloseAndCleanUp<T>(this IConductor<T> parent, T item, bool dispose)
        {
            ScreenExtensions.TryClose(item);

            var itemAsChild = item as IChild;
            if (itemAsChild != null && itemAsChild.Parent == parent)
                itemAsChild.Parent = null;

            if (dispose)
                ScreenExtensions.TryDispose(item);
        }
        
        /// <summary>
        /// For each item in a list, close it, and if its parent is set to the given parent, clear that parent
        /// </summary>
        /// <typeparam name="T">Type of conductor</typeparam>
        /// <param name="parent">Parent</param>
        /// <param name="items">List of items to close and clean up</param>
        /// <param name="dispose">True to dispose children as well as close them</param>
        public static void CloseAndCleanUp<T>(this IConductor<T> parent, IEnumerable items, bool dispose)
        {
            foreach (var item in items.OfType<T>())
            {
                parent.CloseAndCleanUp(item, dispose);
            }
        }
    }
}
