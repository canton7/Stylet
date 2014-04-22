using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletIntegrationTests
{
    public class ShellViewModel : Screen
    {
        private IWindowManager windowManager;

        public ShellViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;

            this.DisplayName = "ShellViewModel";
        }


        private bool? _showDialogAndDialogResultDialogResult;
        public bool? ShowDialogAndDialogResultDialogResult
        {
            get { return this._showDialogAndDialogResultDialogResult; }
            set { SetAndNotify(ref this._showDialogAndDialogResultDialogResult, value);  }
        }

        public void ShowDialogAndDialogResult()
        {
            var dialog = new ShowDialogAndDialogResult.DialogViewModel();
            this.ShowDialogAndDialogResultDialogResult = this.windowManager.ShowDialog(dialog);
        }

        public void ShowWindowsDisplayNameBound()
        {
            var window = new WindowDisplayNameBound.WindowViewModel();
            this.windowManager.ShowWindow(window);
        }

        public void ShowWindowGuardClose()
        {
            var window = new WindowGuardClose.WindowViewModel();
            this.windowManager.ShowWindow(window);
        }

        public void ShowWindowLifecycle()
        {
            var window = new WindowLifecycle.WindowViewModel();
            this.windowManager.ShowWindow(window);
        }

        public void ShowActions()
        {
            var window = new Actions.ActionsViewModel();
            this.windowManager.ShowDialog(window);
        }
    }
}
