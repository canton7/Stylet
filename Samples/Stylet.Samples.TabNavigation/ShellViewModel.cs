using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet.Samples.TabNavigation
{
    class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public ShellViewModel(Page1ViewModel page1, Page2ViewModel page2)
        {
            this.ActivateItem(page1);
            this.Items.Add(page2);
        }
    }
}
