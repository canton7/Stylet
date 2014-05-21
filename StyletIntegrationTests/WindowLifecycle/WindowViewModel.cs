using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletIntegrationTests.WindowLifecycle
{
    public class WindowViewModel : Screen
    {
        private IWindowManager windowManager;

        public BindableCollection<string> Log { get; private set; }

        public WindowViewModel(IWindowManager windowManager)
        {
            this.DisplayName = "Window Lifecycle";

            this.windowManager = windowManager;

            this.Log = new BindableCollection<string>();
        }

        protected override void OnViewLoaded()
        {
            this.Log.Add("View Loaded");
        }

        protected override void OnInitialActivate()
        {
            this.Log.Add("Initial Activate");
        }

        protected override void OnActivate()
        {
            this.Log.Add("Activated");
        }

        protected override void OnDeactivate()
        {
            this.Log.Add("Deactivated");
        }

        protected override void OnClose()
        {
            this.windowManager.ShowMessageBox("Closed", "Closed");
        }
    }
}
