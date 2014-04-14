using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Hidden away slightly, but not too well...
namespace Stylet.SlightlyInternal
{
    public static class ConductorExtensions
    {
        public static void SetParent<T>(this IConductor<T> parent, IEnumerable items, bool setOrClear)
        {
            foreach (var child in items.OfType<IChild>())
            {
                if (setOrClear)
                    child.Parent = parent;
                else if (child.Parent == parent)
                    child.Parent = null;
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
            foreach (var item in items.OfType<IClose>())
            {
                item.Close();
            }

            parent.SetParent(items, false);
        }
    }
}
