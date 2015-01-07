using System;

namespace Stylet.Samples.DesignMode
{
    public class ViewModelLocator
    {
        public UsingViewModelLocatorViewModel UsingViewModelLocatorViewModel
        {
            get
            {
                var vm = new UsingViewModelLocatorViewModel(null);
                vm.TextBoxText = "This is some dummy text.";
                return vm;
            }
        }
    }
}
