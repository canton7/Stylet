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
        private string _name;
        public string Name
        {
            get { return this._name; }
            set { SetAndNotify(ref this._name, value); this.NotifyOfPropertyChange(() => this.CanSayHello); }
        }

        public ShellViewModel()
        {
            this.DisplayName = "Hello, Stylet";
        }

        public bool CanSayHello
        {
            get { return !String.IsNullOrEmpty(this.Name); }
        }
        public void SayHello()
        {
            MessageBox.Show(String.Format("Hello, {0}", this.Name)); // Don't do this
        }
    }
}
