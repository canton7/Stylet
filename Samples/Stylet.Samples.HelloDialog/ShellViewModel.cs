using System;

namespace Stylet.Samples.HelloDialog;

public class ShellViewModel : Screen
{
    private readonly IWindowManager windowManager;
    private readonly IDialogFactory dialogFactory;

    private string _nameString;
    public string NameString
    {
        get => this._nameString;
        set => this.SetAndNotify(ref this._nameString, value);
    }

    public ShellViewModel(IWindowManager windowManager, IDialogFactory dialogFactory)
    {
        this.DisplayName = "Hello Dialog";

        this.windowManager = windowManager;
        this.dialogFactory = dialogFactory;

        this.NameString = "Click the button to show the dialog";
    }

    public void ShowDialog()
    {
        Dialog1ViewModel dialogVm = this.dialogFactory.CreateDialog1();
        bool? result = this.windowManager.ShowDialog(dialogVm);
        if (result.GetValueOrDefault())
            this.NameString = $"Your name is {dialogVm.Name}";
        else
            this.NameString = "Dialog cancelled";
    }
}

public interface IDialogFactory
{
    Dialog1ViewModel CreateDialog1();
}
