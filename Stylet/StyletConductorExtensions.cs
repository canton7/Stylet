using System;
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
        public static void SetParent<T>(this IConductor<T> parent, IEnumerable items)
        {
            foreach (var child in items.OfType<IChild>())
            {
                child.Parent = parent;
            }
        }

        /// <summary>
        /// Close an item, and clear its parent if it's set to the current parent
        /// </summary>
        /// <typeparam name="T">Type of conductor</typeparam>
        /// <param name="parent">Parent</param>
        /// <param name="item">Item to close and clean up</param>
        public static void CloseAndCleanUp<T>(this IConductor<T> parent, T item)
        {
            ScreenExtensions.TryCloseAndDispose(item);

            var itemAsChild = item as IChild;
            if (itemAsChild != null && itemAsChild.Parent == parent)
                itemAsChild.Parent = null;
        }
        
        /// <summary>
        /// For each item in a list, close it, and if its parent is set to the given parent, clear that parent
        /// </summary>
        /// <typeparam name="T">Type of conductor</typeparam>
        /// <param name="parent">Parent</param>
        /// <param name="items">List of items to close and clean up</param>
        public static void CloseAndCleanUp<T>(this IConductor<T> parent, IEnumerable items)
        {
            foreach (var item in items.OfType<T>())
            {
                parent.CloseAndCleanUp(item);
            }
        }
    }
}
