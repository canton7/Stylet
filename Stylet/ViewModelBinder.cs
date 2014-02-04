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
        public static void Bind(FrameworkElement view, object viewModel)
        {
            view.DataContext = viewModel;
            View.SetTarget(view, viewModel);
        }
    }
}
