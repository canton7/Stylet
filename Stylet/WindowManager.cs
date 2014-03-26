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

        class WindowConductor
        {
            private readonly Window window;
            private readonly object viewModel;

            public WindowConductor(Window window, object viewModel)
            {
                this.window = window;
                this.viewModel = viewModel;

                var viewModelAsActivate = viewModel as IActivate;
                if (viewModelAsActivate != null)
                    viewModelAsActivate.Activate();

                var viewModelAsClose = viewModel as IClose;
                if (viewModelAsClose != null)
                {
                    window.Closed += this.WindowClosed;
                    viewModelAsClose.Closed += this.ViewModelClosed;
                }

                if (viewModel is IGuardClose)
                    window.Closing += this.Closing;
            }

            private void WindowClosed(object sender, EventArgs e)
            {
                var viewModelAsClose = (IClose)this.viewModel;

                this.window.Closed -= this.WindowClosed;
                this.window.Closing -= this.Closing; // Not sure this is required
                viewModelAsClose.Closed -= this.ViewModelClosed;

                viewModelAsClose.Close();
            }

            private void ViewModelClosed(object sender, CloseEventArgs e)
            {
                this.window.Closed -= this.WindowClosed;
                this.window.Closing -= this.Closing;
                ((IClose)this.viewModel).Closed -= this.ViewModelClosed;
                this.window.Close();
            }

            private async void Closing(object sender, CancelEventArgs e)
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
                        this.window.Closing -= this.Closing;
                        this.window.Close();
                    }
                }
            }
        }
    }
}
