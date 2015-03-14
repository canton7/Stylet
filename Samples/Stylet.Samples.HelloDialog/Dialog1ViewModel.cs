using System;

namespace Stylet.Samples.HelloDialog
{
    public class Dialog1ViewModel : Screen
    {
        public string Name { get; set; }

        public Dialog1ViewModel()
        {
            this.DisplayName = "I'm Dialog 1";
        }

        public void Close()
        {
            this.RequestClose(true);
        }
    }
}
