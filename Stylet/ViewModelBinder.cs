using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    public static class ViewModelBinder
    {
        public static void Bind(DependencyObject view, object viewModel)
        {
            View.SetTarget(view, viewModel);

            var viewAsFrameworkElement = view as FrameworkElement;
            if (viewAsFrameworkElement != null)
                viewAsFrameworkElement.DataContext = viewModel;
        }
    }
}
