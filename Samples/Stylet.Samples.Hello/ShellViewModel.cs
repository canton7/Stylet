using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet.Samples.Hello
{
    class ShellViewModel : Screen
    {
        public string Name { get; set; }

        public ShellViewModel()
        {
            this.DisplayName = "Hello, Stylet";
        }

        public void SayHello()
        {
            MessageBox.Show(String.Format("Hello, {0}", this.Name)); // Don't do this
        }
    }
}
