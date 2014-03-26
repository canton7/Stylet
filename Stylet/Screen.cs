using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Stylet
{
    public class Screen : PropertyChangedBase, IScreen
    {
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

        private bool _isActive;
        public bool IsActive
        {
            get { return this._isActive; }
            set { SetAndNotify(ref this._isActive, value); }
        }

        void IActivate.Activate()
        {
            if (this.IsActive)
                return;

            this.IsActive = true;

            this.OnActivate();

            var handler = this.Activated;
            if (handler != null)
                handler(this, new ActivationEventArgs());
        }

        protected virtual void OnActivate() { }

        #endregion

        #region IDeactivate

        public event EventHandler<DeactivationEventArgs> Deactivated;

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
            set
            {
                this._parent = value;
                this.NotifyOfPropertyChange();
            }
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
            // Conductor is contravariant, so it's always an IConductor<Screen>, even if they created a Conductor<IScreen> or Conductor<object>
            var conductor = this.Parent as IConductor<Screen>;
            if (conductor != null)
            {
                conductor.CloseItem(this);
                return;
            }

            var viewWindow = this.View as Window;
            if (viewWindow != null)
            {
                if (dialogResult != null)
                    viewWindow.DialogResult = dialogResult;
                viewWindow.Close();
                return;
            }

            throw new InvalidOperationException(String.Format("Unable to close ViewModel {0} as it must have a conductor as a parent, or its view must be a Window", this.GetType().Name));
        }
    }
}
