using System;

namespace Stylet.Samples.DesignMode
{
    public class SelfSetupViewModel : Screen
    {
        private readonly IEventAggregator eventAggregator;

        public string TextBoxText { get; set; }

        /// <summary>
        /// This constructor is called by the designer. Use it to set up dummy values
        /// </summary>
        public SelfSetupViewModel()
        {
            this.TextBoxText = "This is some dummy text.";
        }

        /// <summary>
        /// This constructor is *not* called by the designer, but will be chosen at runtime by StyletIoC
        /// </summary>
        public SelfSetupViewModel(IEventAggregator eventAggregator)
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
