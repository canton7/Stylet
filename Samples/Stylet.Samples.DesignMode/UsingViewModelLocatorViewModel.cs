using System;

namespace Stylet.Samples.DesignMode;

public class UsingViewModelLocatorViewModel : Screen
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly IEventAggregator eventAggregator;
#pragma warning restore IDE0052 // Remove unread private members

    public string TextBoxText { get; set; }

    public UsingViewModelLocatorViewModel(IEventAggregator eventAggregator)
    {
        this.eventAggregator = eventAggregator;
    }

    public bool CanDoSomething => false;

    public void DoSomething()
    {
    }
}
