using Stylet.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    /// <summary>
    /// Implementation of IScreen. Useful as a base class for your ViewModels
    /// </summary>
    public class Screen : ValidatingModelBase, IScreen
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="Screen"/> class, without setting up a validator
        /// </summary>
        public Screen() : this(null) { }

        /// <summary>
        /// Initialises a new instance of the <see cref="Screen"/> class, which can validate properties using the given validator
        /// </summary>
        /// <param name="validator">Validator to use</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Can be safely called from the Ctor, as it doesn't depend on state being set")]
        public Screen(IModelValidator validator) : base(validator)
        {
            var type = this.GetType();
            this.DisplayName = type.FullName;
            this.logger = LogManager.GetLogger(type);
        }

        #region IHaveDisplayName

        private string _displayName;

        /// <summary>
        /// Gets or sets the name associated with this ViewModel.
        /// Shown e.g. in a window's title bar, or as a tab's displayName
        /// </summary>
        public string DisplayName
        {
            get { return this._displayName; }
            set { this.SetAndNotify(ref this._displayName, value); }
        }

        #endregion

        #region IActivate

        /// <summary>
        /// Fired whenever the Screen is activated
        /// </summary>
        public event EventHandler<ActivationEventArgs> Activated;

        private bool hasBeenActivatedEver;

        private bool _isActive;

        /// <summary>
        /// Gets or sets a value indicating whether this Screen is currently active
        /// </summary>
        public bool IsActive
        {
            get { return this._isActive; }
            set { this.SetAndNotify(ref this._isActive, value); }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "As this is a framework type, don't want to make it too easy for users to call this method")]
        void IActivate.Activate()
        {
            if (this.IsActive)
                return;

            this.IsActive = true;
            this.isClosed = false;

            this.logger.Info("Activating");

            if (!this.hasBeenActivatedEver)
                this.OnInitialActivate();
            this.hasBeenActivatedEver = true;

            this.OnActivate();

            var handler = this.Activated;
            if (handler != null)
                handler(this, new ActivationEventArgs());
        }

        /// <summary>
        /// Called the very first time this Screen is activated, and never again
        /// </summary>
        protected virtual void OnInitialActivate() { }

        /// <summary>
        /// Called every time this screen is activated
        /// </summary>
        protected virtual void OnActivate() { }

        #endregion

        #region IDeactivate

        /// <summary>
        /// Fired whenever the Screen is deactivated
        /// </summary>
        public event EventHandler<DeactivationEventArgs> Deactivated;

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Stylet.Screen.#Stylet.IDeactivate.Deactivate()", Justification = "As this is a framework type, don't want to make it too easy for users to call this method")]
        void IDeactivate.Deactivate()
        {
            if (!this.IsActive)
                return;

            this.IsActive = false;
            this.isClosed = false;

            this.logger.Info("Deactivating");

            this.OnDeactivate();

            var handler = this.Deactivated;
            if (handler != null)
                handler(this, new DeactivationEventArgs());
        }

        /// <summary>
        /// Called every time this screen is deactivated
        /// </summary>
        protected virtual void OnDeactivate() { }

        #endregion

        #region IClose

        private bool isClosed;

        /// <summary>
        /// Called whenever this Screen is closed
        /// </summary>
        public event EventHandler<CloseEventArgs> Closed;

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "As this is a framework type, don't want to make it too easy for users to call this method")]
        void IClose.Close()
        {
            if (this.isClosed)
                return;

            // This will early-exit if it's already deactive
            ((IDeactivate)this).Deactivate();

            this.View = null;
            this.isClosed = true;

            this.logger.Info("Closing");

            this.OnClose();

            var handler = this.Closed;
            if (handler != null)
                handler(this, new CloseEventArgs());
        }

        /// <summary>
        /// Called when this screen is closed
        /// </summary>
        protected virtual void OnClose() { }

        #endregion

        #region IViewAware

        /// <summary>
        /// Gets the View attached to this ViewModel, if any. Using this should be a last resort
        /// </summary>
        public UIElement View { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "As this is a framework type, don't want to make it too easy for users to call this method")]
        void IViewAware.AttachView(UIElement view)
        {
            if (this.View != null)
                throw new InvalidOperationException(String.Format("Tried to attach View {0} to ViewModel {1}, but it already has a view attached", view.GetType().Name, this.GetType().Name));

            this.View = view;

            this.logger.Info("Attaching view {0}", view);

            var viewAsFrameworkElement = view as FrameworkElement;
            if (viewAsFrameworkElement != null)
            {
                if (viewAsFrameworkElement.IsLoaded)
                    this.OnViewLoaded();
                else
                    viewAsFrameworkElement.Loaded += (o, e) => this.OnViewLoaded();
            }
        }

        /// <summary>
        /// Called when the view attaches to the Screen loads
        /// </summary>
        protected virtual void OnViewLoaded() { }

        #endregion

        #region IChild

        private object _parent;

        /// <summary>
        /// Gets or sets the parent conductor of this screen. Used to RequestClose to request a closure
        /// </summary>
        public object Parent
        {
            get { return this._parent; }
            set { this.SetAndNotify(ref this._parent, value); }
        }

        #endregion

        #region IGuardClose

        /// <summary>
        /// Called when a conductor wants to know whether this screen can close.
        /// </summary>
        /// <remarks>Internally, this calls CanClose, and wraps the response in a Task</remarks>
        /// <returns>A task returning true (can close) or false (can't close)</returns>
        public virtual Task<bool> CanCloseAsync()
        {
            return Task.FromResult(this.CanClose());
        }

        /// <summary>
        /// Synchronous alternative to CanClose
        /// </summary>
        /// <returns>True if this screen can close, or false otherwise</returns>
        protected virtual bool CanClose()
        {
            return true; 
        }

        #endregion

        #region IRequestClose

        /// <summary>
        /// Request that the conductor responsible for this screen close it
        /// </summary>
        /// <param name="dialogResult">DialogResult to return, if this is a dialog</param>
        public virtual void RequestClose(bool? dialogResult = null)
        {
            var conductor = this.Parent as IChildDelegate;
            if (conductor != null)
            {
                this.logger.Info("RequstClose called. Conductor: {0}; DialogResult: {1}", conductor, dialogResult);
                conductor.CloseItem(this, dialogResult);
            }
            else
            {
                var e = new InvalidOperationException(String.Format("Unable to close ViewModel {0} as it must have a conductor as a parent (note that windows and dialogs automatically have such a parent)", this.GetType()));
                this.logger.Error(e);
                throw e;
            }
        }

        #endregion

        /// <summary>
        /// Obselete - use RequestClose
        /// </summary>
        /// <param name="dialogResult">DialogResult to return, if this is a dialog</param>
        [Obsolete("Obseleted by RequestClose")]
        public virtual void TryClose(bool? dialogResult = null)
        {
            this.RequestClose(dialogResult);
        }
    }
}
