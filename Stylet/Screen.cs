using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Stylet
{
    /// <summary>
    /// Implementation of IScreen. Useful as a base class for your ViewModels
    /// </summary>
    public class Screen : ValidatingModelBase, IScreen
    {
        public Screen() : this(null) { }
        public Screen(IModelValidator validator) : base(validator)
        {
            this.DisplayName = this.GetType().FullName;
        }

        #region WeakEventManager

        private IWeakEventManager _weakEventManager;
        /// <summary>
        /// WeakEventManager owned by this screen (lazy)
        /// </summary>
        protected virtual IWeakEventManager weakEventManager
        {
            get
            {
                if (this._weakEventManager == null)
                    this._weakEventManager = new WeakEventManager();
                return this._weakEventManager;
            }
        }

        /// <summary>
        /// Proxy around this.weakEventManager.BindWeak. Binds to an INotifyPropertyChanged source, in a way which doesn't cause us to be retained
        /// </summary>
        /// <example>this.BindWeak(objectToBindTo, x => x.PropertyToBindTo, newValue => handlerForNewValue)</example>
        /// <param name="source">Object to observe for PropertyChanged events</param>
        /// <param name="selector">Expression for selecting the property to observe, e.g. x => x.PropertyName</param>
        /// <param name="handler">Handler to be called when that property changes</param>
        /// <returns>A resource which can be used to undo the binding</returns>
        protected virtual IEventBinding BindWeak<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> selector, Action<TProperty> handler)
            where TSource : class, INotifyPropertyChanged
        {
            return this.weakEventManager.BindWeak(source, selector, handler);
        }

        #endregion

        #region IHaveDisplayName

        private string _displayName;
        public virtual string DisplayName
        {
            get { return this._displayName; }
            set { SetAndNotify(ref this._displayName, value); }
        }

        #endregion

        #region IActivate

        public event EventHandler<ActivationEventArgs> Activated;

        private bool hasBeenActivatedEver;

        private bool _isActive;
        /// <summary>
        /// True if this Screen is currently active
        /// </summary>
        public bool IsActive
        {
            get { return this._isActive; }
            set { SetAndNotify(ref this._isActive, value); }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "As this is a framework type, don't want to make it too easy for users to call this method")]
        void IActivate.Activate()
        {
            if (this.IsActive)
                return;

            this.IsActive = true;

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

        public event EventHandler<DeactivationEventArgs> Deactivated;

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Scope = "member", Target = "Stylet.Screen.#Stylet.IDeactivate.Deactivate()", Justification = "As this is a framework type, don't want to make it too easy for users to call this method")]
        void IDeactivate.Deactivate()
        {
            if (!this.IsActive)
                return;

            this.IsActive = false;

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

        public event EventHandler<CloseEventArgs> Closed;

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "As this is a framework type, don't want to make it too easy for users to call this method")]
        void IClose.Close()
        {
            // This will early-exit if it's already deactive
            ((IDeactivate)this).Deactivate();

            this.View = null;

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

        public UIElement View { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "As this is a framework type, don't want to make it too easy for users to call this method")]
        void IViewAware.AttachView(UIElement view)
        {
            if (this.View != null)
                throw new Exception(String.Format("Tried to attach View {0} to ViewModel {1}, but it already has a view attached", view.GetType().Name, this.GetType().Name));

            this.View = view;

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
        /// Parent conductor of this screen. Used to TryClose to request a closure
        /// </summary>
        public object Parent
        {
            get { return this._parent; }
            set { SetAndNotify(ref this._parent, value); }
        }

        #endregion

        #region IGuardClose

        /// <summary>
        /// Called when a conductor wants to know whether this screen can close.
        /// </summary>
        /// <returns>A task returning true (can close) or false (can't close)</returns>
        public virtual Task<bool> CanCloseAsync()
        {
            return Task.FromResult(true);
        }

        #endregion

        /// <summary>
        /// Request that the conductor responsible for this screen close it
        /// </summary>
        /// <param name="dialogResult"></param>
        public virtual void TryClose(bool? dialogResult = null)
        {
            var conductor = this.Parent as IChildDelegate;
            if (conductor != null)
                conductor.CloseItem(this, dialogResult);
            else
                throw new InvalidOperationException(String.Format("Unable to close ViewModel {0} as it must have a conductor as a parent (note that windows and dialogs automatically have such a parent)", this.GetType().Name));
        }
    }
}
