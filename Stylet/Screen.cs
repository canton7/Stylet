using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Stylet
{
    public class Screen : PropertyChangedBase, IScreen
    {
        private Lazy<IWeakEventManager> lazyWeakEventManager = new Lazy<IWeakEventManager>(() => new WeakEventManager(), true);
        protected IWeakEventManager weakEventManager { get { return this.lazyWeakEventManager.Value; } }

        #region IHaveDisplayName

        private string _displayName;
        public string DisplayName
        {
            get { return this._displayName; }
            set { SetAndNotify(ref this._displayName, value); }
        }

        #endregion

        #region IActivate

        public event EventHandler<ActivationEventArgs> Activated;

        private bool hasBeenActivatedEver;

        private bool _isActive;
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

        protected virtual void OnInitialActivate() { }
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

        protected virtual void OnViewLoaded() { }

        #endregion

        #region IChild

        private object _parent;
        public object Parent
        {
            get { return this._parent; }
            set { SetAndNotify(ref this._parent, value); }
        }

        #endregion

        #region IGuardClose

        public virtual Task<bool> CanCloseAsync()
        {
            return Task.FromResult(true);
        }

        #endregion

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
