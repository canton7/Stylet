using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    public interface IViewAware
    {
        object View { get; }
        void AttachView(object view);
    }

    public interface IScreen : IViewAware
    {
    }

    public class Screen : IScreen
    {
        public virtual void TryClose(bool? dialogResult = null)
        {
            // TODO: Check for parent conductor
            var viewWindow = this.View as Window;
            if (viewWindow != null)
            {
                if (dialogResult != null)
                    viewWindow.DialogResult = dialogResult;
                viewWindow.Close();
                return;
            }

            throw new InvalidOperationException(String.Format("Unable to close ViewModel {0} as it must have a parent, or its view must be a Window", this.GetType().Name));
        }

        #region IViewAware

        public object View { get; private set; }

        void IViewAware.AttachView(object view)
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
    }
}
