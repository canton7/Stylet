using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Stylet.Samples.OverridingViewManager
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    [ViewModel(typeof(ShellViewModel))]
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
        }
    }
}
