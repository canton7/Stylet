using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletIntegrationTests.WindowGuardClose
{
    public class WindowViewModel : Screen
    {
        public bool AllowClose { get; set; }

        public WindowViewModel()
        {
            this.DisplayName = "Window Guard Close";
        }

        public override Task<bool> CanCloseAsync()
        {
            return this.AllowClose ? Task.Delay(2000).ContinueWith(t => true) : Task.FromResult(false);
        }
    }
}
