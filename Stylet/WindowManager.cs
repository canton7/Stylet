using System;
using System.Collections.Generic;
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
            var view = ViewLocator.LocateForModel(viewModel) as Window;
            if (view == null)
                throw new Exception(String.Format("Tried to show {0} as a window, but it isn't a Window", view.GetType().Name));

            ViewModelBinder.Bind(view, viewModel);

            var haveDisplayName = viewModel as IHaveDisplayName;
            if (haveDisplayName != null)
            {
                var binding = new Binding("DisplayName") { Mode = BindingMode.TwoWay };
                view.SetBinding(Window.TitleProperty, binding);
            }

            if (isDialog)
            {
                var owner = this.InferOwnerOf(view);
                if (owner != null)
                    view.Owner = owner;
            }

            return view;
        }

        private Window InferOwnerOf(Window window)
        {
            if (Application.Current == null)
                return null;

            var active = Application.Current.Windows.OfType<Window>().Where(x => x.IsActive).FirstOrDefault() ?? Application.Current.MainWindow;
            return active == window ? null : active;
        }
    }
}
