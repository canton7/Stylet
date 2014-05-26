using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    // Don't name ConductorExtensions, otherwise it's too obvious when someone types 'Conductor'
    public static class StyletConductorExtensions
    {
        public static void SetParent<T>(this IConductor<T> parent, IEnumerable items)
        {
            foreach (var child in items.OfType<IChild>())
            {
                child.Parent = parent;
            }
        }

        /// <summary>
        /// Close an item, and clean it up a bit
        /// </summary>
        public static void CloseAndCleanUp<T>(this IConductor<T> parent, T item)
        {
            ScreenExtensions.TryClose(item);

            var itemAsChild = item as IChild;
            if (itemAsChild != null && itemAsChild.Parent == parent)
                itemAsChild.Parent = null;
        }
        
        public static void CloseAndCleanUp<T>(this IConductor<T> parent, IEnumerable items)
        {
            foreach (var item in items.OfType<T>())
            {
                parent.CloseAndCleanUp(item);
            }
        }
    }
}
