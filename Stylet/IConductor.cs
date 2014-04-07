using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    /// <summary>
    /// Generalised parent, with many children
    /// </summary>
    public interface IParent<out T>
    {
        /// <summary>
        /// Fetch all children of this parent
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetChildren();
    }

    /// <summary>
    /// Thing which has a single active item
    /// </summary>
    /// <typeparam name="T">Type of the active item</typeparam>
    public interface IHaveActiveItem<T>
    {
        T ActiveItem { get; set; }
    }

    /// <summary>
    /// Thing which has one or more children, and from which a child can request that it be closed
    /// </summary>
    public interface IChildDelegate
    {
        void CloseItem(object item, bool? dialogResult = null);
    }

    /// <summary>
    /// Thing which owns one or more children, and can manage their lifecycles accordingly
    /// </summary>
    public interface IConductor<T>
    {
        /// <summary>
        /// Active the given item
        /// </summary>
        /// <param name="item">Item to activate</param>
        void ActivateItem(T item);

        /// <summary>
        /// Deactivate the given item
        /// </summary>
        /// <param name="item">Item to deactivate</param>
        void DeactivateItem(T item);

        /// <summary>
        /// Close the given item
        /// </summary>
        /// <param name="item">Item to close</param>
        void CloseItem(T item);
    }
}
