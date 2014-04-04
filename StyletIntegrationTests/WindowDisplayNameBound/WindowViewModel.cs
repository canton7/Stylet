using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletIntegrationTests.WindowDisplayNameBound
{
    public class WindowViewModel : Screen
    {
        private int count = 0;

        public WindowViewModel()
        {
            this.DisplayName = String.Format("Count: {0}", this.count);
        }

        public void AddCount()
        {
            this.count++;
            this.DisplayName = String.Format("Count: {0}", this.count);
        }
    }
}
