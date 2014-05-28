using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;

namespace Stylet
{
    /// <summary>
    /// Manager capable of taking a ViewModel instance, instantiating its View and showing it as a dialog or window
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Given a ViewModel, show its corresponding View as a window
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        void ShowWindow(object viewModel);

        /// <summary>
        /// Given a ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>DialogResult of the View</returns>
        bool? ShowDialog(object viewModel);
    }

    /// <summary>
    /// Default implementation of IWindowManager, is capable of showing a ViewModel's View as a dialog or a window
    /// </summary>
    public class WindowManager : IWindowManager
    {
        private IViewManager viewManager;

        /// <summary>
        /// Create a new WindowManager instance, using the given IViewManager
        /// </summary>
        /// <param name="viewManager">IViewManager to use when creating views</param>
        public WindowManager(IViewManager viewManager)
        {
            this.viewManager = viewManager;
        }

        /// <summary>
        /// Given a ViewModel, show its corresponding View as a window
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        public void ShowWindow(object viewModel)
        {
            this.CreateWindow(viewModel, false).Show();
        }

        /// <summary>
        /// Given a ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>DialogResult of the View</returns>
        public bool? ShowDialog(object viewModel)
        {
            return this.CreateWindow(viewModel, true).ShowDialog();
        }

        /// <summary>
        /// Given a ViewModel, create its View, ensure that it's a Window, and set it up
        /// </summary>
        /// <param name="viewModel">ViewModel to create the window for</param>
        /// <param name="isDialog">True if the window will be used as a dialog</param>
        /// <returns>Window which was created and set up</returns>
        protected virtual Window CreateWindow(object viewModel, bool isDialog)
        {
            var view = this.viewManager.CreateAndSetupViewForModel(viewModel);
            var window = view as Window;
            if (window == null)
                throw new ArgumentException(String.Format("Tried to show {0} as a window, but it isn't a Window", view == null ? "(null)" : view.GetType().Name));

            this.viewManager.BindViewToModel(window, viewModel);

            var haveDisplayName = viewModel as IHaveDisplayName;
            if (haveDisplayName != null && BindingOperations.GetBindingBase(window, Window.TitleProperty) == null)
            {
                var binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
                window.SetBinding(Window.TitleProperty, binding);
            }

            if (isDialog)
            {
                var owner = this.InferOwnerOf(window);
                if (owner != null)
                    window.Owner = owner;
            }

            new WindowConductor(window, viewModel);

            return window;
        }

        private Window InferOwnerOf(Window window)
        {
            if (Application.Current == null)
                return null;

            var active = Application.Current.Windows.OfType<Window>().Where(x => x.IsActive).FirstOrDefault() ?? Application.Current.MainWindow;
            return active == window ? null : active;
        }

        class WindowConductor : IChildDelegate
        {
            private readonly Window window;
            private readonly object viewModel;

            public WindowConductor(Window window, object viewModel)
            {
                this.window = window;
                this.viewModel = viewModel;

                // They won't be able to request a close unless they implement IChild anyway...
                var viewModelAsChild = this.viewModel as IChild;
                if (viewModelAsChild != null)
                    viewModelAsChild.Parent = this;

                ScreenExtensions.TryActivate(this.viewModel);

                var viewModelAsClose = this.viewModel as IClose;
                if (viewModelAsClose != null)
                    window.Closed += this.WindowClosed;

                if (this.viewModel is IGuardClose)
                    window.Closing += this.WindowClosing;

                if (this.viewModel is IActivate || this.viewModel is IDeactivate)
                    window.StateChanged += WindowStateChanged;
            }

            void WindowStateChanged(object sender, EventArgs e)
            {
                switch (this.window.WindowState)
                {
                    case WindowState.Maximized:
                    case WindowState.Normal:
                        ScreenExtensions.TryActivate(this.viewModel);
                        break;

                    case WindowState.Minimized:
                        ScreenExtensions.TryDeactivate(this.viewModel);
                        break;
                }
            }

            private void WindowClosed(object sender, EventArgs e)
            {
                this.window.StateChanged -= this.WindowStateChanged;
                this.window.Closed -= this.WindowClosed;
                this.window.Closing -= this.WindowClosing; // Not sure this is required

                ScreenExtensions.TryClose(this.viewModel);
            }

            /// <summary>
            /// Closing event from the window
            /// </summary>
            private async void WindowClosing(object sender, CancelEventArgs e)
            {
                if (e.Cancel)
                    return;

                // See if the task completed synchronously
                var task = ((IGuardClose)this.viewModel).CanCloseAsync();
                if (task.IsCompleted)
                {
                    e.Cancel = !task.Result;
                }
                else
                {
                    e.Cancel = true;
                    if (await task)
                    {
                        this.window.Closing -= this.WindowClosing;
                        this.window.Close();
                        // The Closed event handler handles unregistering the events, and closing the ViewModel
                    }
                }
            }

            /// <summary>
            /// Close was requested by the child
            /// </summary>
            async void IChildDelegate.CloseItem(object item, bool? dialogResult)
            {
                if (item != this.viewModel)
                    return;

                var guardClose = this.viewModel as IGuardClose;
                if (guardClose != null && !await guardClose.CanCloseAsync())
                    return;

                if (dialogResult != null)
                    this.window.DialogResult = dialogResult;

                this.window.StateChanged -= this.WindowStateChanged;
                this.window.Closed -= this.WindowClosed;
                this.window.Closing -= this.WindowClosing;

                ScreenExtensions.TryClose(this.viewModel);

                this.window.Close();
            }
        }
    }
}
