using System;
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
        /// Gets the view associated with this ViewModel
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
    /// State in which a screen can be
    /// </summary>
    public enum ScreenState
    {
        /// <summary>
        /// Deprecated: Screens now start in Deactivated
        /// </summary>
        [Obsolete("Screens now start in the Deactivated state")]
        Initial,

        /// <summary>
        /// Screen is active. It is likely being displayed to the user
        /// </summary>
        Active,

        /// <summary>
        /// Screen is deactivated. It is either new, has been hidden in favour of another Screen, or the entire window has been minimised
        /// </summary>
        Deactivated,

        /// <summary>
        /// Screen has been closed. It has no associated View, but may yet be displayed again
        /// </summary>
        Closed,
    }

    /// <summary>
    /// Has a concept of state, which can be manipulated by its Conductor
    /// </summary>
    public interface IScreenState
    {
        /// <summary>
        /// Gets the current state of the Screen
        /// </summary>
        ScreenState ScreenState { get; }

        /// <summary>
        /// Gets a value indicating whether the current state is ScreenState.Active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Raised when the Screen's state changed, for any reason
        /// </summary>
        event EventHandler<ScreenStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Raised when the object is actually activated
        /// </summary>
        event EventHandler<ActivationEventArgs> Activated;

        /// <summary>
        /// Raised when the object is actually deactivated
        /// </summary>
        event EventHandler<DeactivationEventArgs> Deactivated;

        /// <summary>
        /// Raised when the object is actually closed
        /// </summary>
        event EventHandler<CloseEventArgs> Closed;

        /// <summary>
        /// Activate the object. May not actually cause activation (e.g. if it's already active)
        /// </summary>
        void Activate();

        /// <summary>
        /// Deactivate the object. May not actually cause deactivation (e.g. if it's already deactive)
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Close the object. May not actually cause closure (e.g. if it's already closed)
        /// </summary>
        void Close();
    }

    /// <summary>
    /// Has a display name. In reality, this is bound to things like Window titles and TabControl tabs
    /// </summary>
    public interface IHaveDisplayName
    {
        /// <summary>
        /// Gets or sets the name which should be displayed
        /// </summary>
        string DisplayName { get; set; }
    }

    /// <summary>
    /// Acts as a child. Knows about its parent
    /// </summary>
    public interface IChild
    {
        /// <summary>
        /// Gets or sets the parent object to this child
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
        /// <returns>A task indicating whether the object can close</returns>
        Task<bool> CanCloseAsync();
    }

    /// <summary>
    /// Get the object to request that its parent close it
    /// </summary>
    public interface IRequestClose
    {
        /// <summary>
        /// Request that the conductor responsible for this screen close it
        /// </summary>
        /// <param name="dialogResult">DialogResult to return, if this is a dialog</param>
        void RequestClose(bool? dialogResult = null);
    }

    /// <summary>
    /// Generalised 'screen' composing all the behaviours expected of a screen
    /// </summary>
    public interface IScreen : IViewAware, IHaveDisplayName, IScreenState, IChild, IGuardClose, IRequestClose
    {
    }

    /// <summary>
    /// EventArgs associated with the IScreenState.StateChanged event
    /// </summary>
    public class ScreenStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the state being transitioned to
        /// </summary>
        public ScreenState NewState { get; private set; }

        /// <summary>
        /// Gets the state being transitioned away from
        /// </summary>
        public ScreenState PreviousState { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="ScreenStateChangedEventArgs"/> class
        /// </summary>
        /// <param name="newState">State being transitioned to</param>
        /// <param name="previousState">State being transitioned away from</param>
        public ScreenStateChangedEventArgs(ScreenState newState, ScreenState previousState)
        {
            this.NewState = newState;
            this.PreviousState = previousState;
        }
    }

    /// <summary>
    /// EventArgs associated with the IScreenState.Activated event
    /// </summary>
    public class ActivationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether this is the first time this Screen has been activated, ever
        /// </summary>
        public bool IsInitialActivate { get; private set; }

        /// <summary>
        /// Gets the state being transitioned away from
        /// </summary>
        public ScreenState PreviousState { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="ActivationEventArgs"/> class
        /// </summary>
        /// <param name="previousState">State being transitioned away from</param>
        /// <param name="isInitialActivate">True if this is the first time this screen has ever been activated</param>
        public ActivationEventArgs(ScreenState previousState, bool isInitialActivate)
        {
            this.IsInitialActivate = isInitialActivate;
            this.PreviousState = previousState;
        }
    }

    /// <summary>
    /// EventArgs associated with the IScreenState.Deactivated event
    /// </summary>
    public class DeactivationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the state being transitioned away from
        /// </summary>
        public ScreenState PreviousState { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="DeactivationEventArgs"/> class
        /// </summary>
        /// <param name="previousState">State being transitioned away from</param>
        public DeactivationEventArgs(ScreenState previousState)
        {
            this.PreviousState = previousState;
        }
    }

    /// <summary>
    /// EventArgs associated with the IScreenState.Closed event
    /// </summary>
    public class CloseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the state being transitioned away from
        /// </summary>
        public ScreenState PreviousState { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="CloseEventArgs"/> class
        /// </summary>
        /// <param name="previousState">State being transitioned away from</param>
        public CloseEventArgs(ScreenState previousState)
        {
            this.PreviousState = previousState;
        }
    }
}
