using System;

namespace Stylet.Samples.Hello;

public class ShellViewModel : Screen
{
    private readonly IWindowManager windowManager;

    private string _name;
    public string Name
    {
        get => this._name;
        set
        {
            this.SetAndNotify(ref this._name, value);
            this.NotifyOfPropertyChange(() => this.CanSayHello);
        }
    }

    public ShellViewModel(IWindowManager windowManager)
    {
        this.DisplayName = "Hello, Stylet";
        this.windowManager = windowManager;
    }

    public bool CanSayHello => !string.IsNullOrEmpty(this.Name);
    public void SayHello()
    {
        this.windowManager.ShowMessageBox($"Hello, {this.Name}");
    }
}
