using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet.Samples.HelloDialog
{
    public class ShellViewModel : Screen
    {
        private IWindowManager windowManager;
        private IDialogFactory dialogFactory;

        public ShellViewModel(IWindowManager windowManager, IDialogFactory dialogFactory)
        {
            this.DisplayName = "Hello Dialog";

            this.windowManager = windowManager;
            this.dialogFactory = dialogFactory;
        }

        public void ShowDialog()
        {
            var dialogVm = this.dialogFactory.CreateDialog1();
            this.windowManager.ShowDialog(dialogVm);
        }
    }

    public interface IDialogFactory
    {
        Dialog1ViewModel CreateDialog1();
    }
}
