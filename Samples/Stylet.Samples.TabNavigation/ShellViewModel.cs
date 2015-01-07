using System;

namespace Stylet.Samples.TabNavigation
{
    class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public ShellViewModel(Page1ViewModel page1, Page2ViewModel page2)
        {
            this.Items.Add(page1);
            this.Items.Add(page2);

            this.ActiveItem = page1;
        }
    }
}
