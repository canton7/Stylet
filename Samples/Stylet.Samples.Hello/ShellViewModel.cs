using System;

namespace Stylet.Samples.Hello
{
    class ShellViewModel : Screen
    {
        private IWindowManager windowManager;

        private string _name;
        public string Name
        {
            get { return this._name; }
            set
            {
                SetAndNotify(ref this._name, value);
                this.NotifyOfPropertyChange(() => this.CanSayHello);
            }
        }

        public ShellViewModel(IWindowManager windowManager)
        {
            this.DisplayName = "Hello, Stylet";
            this.windowManager = windowManager;
        }

        public bool CanSayHello
        {
            get { return !String.IsNullOrEmpty(this.Name); }
        }
        public void SayHello()
        {
            this.windowManager.ShowMessageBox(String.Format("Hello, {0}", this.Name));
        }
    }
}
