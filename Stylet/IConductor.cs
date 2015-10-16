using System.Collections.Generic;

namespace Stylet
{
    /// <summary>
    /// Generalised parent, with many children
    /// </summary>
    /// <typeparam name="T">Type of children</typeparam>
    public interface IParent<out T>
    {
        /// <summary>
        /// Fetch all children of this parent
        /// </summary>
        /// <returns>All children owned by this parent</returns>
        IEnumerable<T> GetChildren();
    }

    /// <summary>
    /// Thing which has a single active item
    /// </summary>
    /// <typeparam name="T">Type of the active item</typeparam>
    public interface IHaveActiveItem<T>
    {
        /// <summary>
        /// Gets or sets the only item which is currently active.
        /// This normally corresponds to the item being displayed
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
    /// <typeparam name="T">Type of child being conducted</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    // Not sure whether this might change in future...
    public interface IConductor<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to dispose a child when it's closed. True by default
        /// </summary>
        bool DisposeChildren { get; set; }

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
