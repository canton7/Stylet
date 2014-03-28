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
    public interface IWindowManager
    {
        void ShowWindow(object viewModel);
        bool? ShowDialog(object viewModel);
    }

    public class WindowManager : IWindowManager
    {
        public void ShowWindow(object viewModel)
        {
            this.CreateWindow(viewModel, false).Show();
        }

        public bool? ShowDialog(object viewModel)
        {
            return this.CreateWindow(viewModel, true).ShowDialog();
        }

        private Window CreateWindow(object viewModel, bool isDialog)
        {
            var viewManager = IoC.Get<IViewManager>();

            var view = viewManager.LocateViewForModel(viewModel);
            var window = view as Window;
            if (window == null)
                throw new Exception(String.Format("Tried to show {0} as a window, but it isn't a Window", view.GetType().Name));

            viewManager.BindViewToModel(window, viewModel);

            var haveDisplayName = viewModel as IHaveDisplayName;
            if (haveDisplayName != null)
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
                        this.window.StateChanged -= this.WindowStateChanged;
                        ScreenExtensions.TryClose(this.viewModel);
                        this.window.Close();
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
