using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    /// <summary>
    /// Is aware of the fact that it has a view
    /// </summary>
    public interface IViewAware
    {
        /// <summary>
        /// The view associated with this ViewModel
        /// </summary>
        UIElement View { get; }

        /// <summary>
        /// Called when the view should be attached. Should set View property.
        /// </summary>
        /// <remarks>Separate from the View property so it can be explicitely implemented</remarks>
        /// <param name="view">View to attach</param>
        void AttachView(UIElement view);
    }

    /// <summary>
    /// Can be activated, and raises an event when it is actually activated
    /// </summary>
    public interface IActivate
    {
        /// <summary>
        /// Activate the object. May not actually cause activation (e.g. if it's already active)
        /// </summary>
        void Activate();
        
        /// <summary>
        /// Raised when the object is actually activated
        /// </summary>
        event EventHandler<ActivationEventArgs> Activated;
    }

    /// <summary>
    /// Can be deactivated, and raises an event when it is actually deactivated
    /// </summary>
    public interface IDeactivate
    {
        /// <summary>
        /// Deactivate the object. May not actually cause deactivation (e.g. if it's already deactive)
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Raised when the object is actually deactivated
        /// </summary>
        event EventHandler<DeactivationEventArgs> Deactivated;
    }

    /// <summary>
    /// Can be closed, and raises an event when it is actually closed
    /// </summary>
    public interface IClose
    {
        /// <summary>
        /// Close the object. May not actually cause closure (e.g. if it's already closed)
        /// </summary>
        void Close();

        /// <summary>
        /// Raised when the object is actually closed
        /// </summary>
        event EventHandler<CloseEventArgs> Closed;
    }

    /// <summary>
    /// Has a display name. In reality, this is bound to things like Window titles and TabControl tabs
    /// </summary>
    public interface IHaveDisplayName
    {
        /// <summary>
        /// Name which should be displayed
        /// </summary>
        string DisplayName { get; set; }
    }

    /// <summary>
    /// Acts as a child. Knows about its parent
    /// </summary>
    public interface IChild
    {
        /// <summary>
        /// Parent object to this child
        /// </summary>
        object Parent { get; set; }
    }

    /// <summary>
    /// Has an opinion on whether it should be closed
    /// </summary>
    /// <remarks>If implemented, CanCloseAsync should be called prior to closing the object</remarks>
    public interface IGuardClose
    {
        /// <summary>
        /// Returns whether or not the object can close, potentially asynchronously
        /// </summary>
        Task<bool> CanCloseAsync();
    }

    /// <summary>
    /// Generalised 'screen' composing all the behaviours expected of a screen
    /// </summary>
    public interface IScreen : IViewAware, IHaveDisplayName, IActivate, IDeactivate, IChild, IClose, IGuardClose
    {
    }

    /// <summary>
    /// EventArgs associated with the IActivate.Activated event
    /// </summary>
    public class ActivationEventArgs : EventArgs
    {
    }

    /// <summary>
    /// EventArgs associated with the IDeactivate.Deactivated event
    /// </summary>
    public class DeactivationEventArgs : EventArgs
    {
    }

    /// <summary>
    /// EventArgs associated with the IClose.Closed event
    /// </summary>
    public class CloseEventArgs : EventArgs
    {
    }
}
