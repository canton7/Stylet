using System;
using System.Collections.Generic;

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
        /// <summary>
        /// Only item which is currently active. This normally corresponds to the item being displayed
        /// </summary>
        T ActiveItem { get; set; }
    }

    /// <summary>
    /// Thing which has one or more children, and from which a child can request that it be closed
    /// </summary>
    public interface IChildDelegate
    {
        /// <summary>
        /// Called by the child to request that is be closed
        /// </summary>
        /// <param name="item">Child object, which is passed by the child itself</param>
        /// <param name="dialogResult">DialogResult to use to close, if any</param>
        void CloseItem(object item, bool? dialogResult = null);
    }

    /// <summary>
    /// Thing which owns one or more children, and can manage their lifecycles accordingly
    /// </summary>
    public interface IConductor<T>
    {
        /// <summary>
        /// Activate the given item
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
