using System;

namespace Stylet.Samples.DesignMode
{
    public class UsingViewModelLocatorViewModel : Screen
    {
       private readonly IEventAggregator eventAggregator;

        public string TextBoxText { get; set; }

        public UsingViewModelLocatorViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public bool CanDoSomething
        {
            get { return false; }
        }

        public void DoSomething()
        {
        }
    }
}
