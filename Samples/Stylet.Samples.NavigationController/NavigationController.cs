using Stylet.Samples.NavigationController.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet.Samples.NavigationController;

public interface INavigationController
{
    void NavigateToPage1();
    void NavigateToPage2(string initiator);
}

public interface INavigationControllerDelegate
{
    void NavigateTo(IScreen screen);
}

public class NavigationController : INavigationController
{
    private readonly Func<Page1ViewModel> page1ViewModelFactory;
    private readonly Func<Page2ViewModel> page2ViewModelFactory;

    public INavigationControllerDelegate Delegate { get; set; }

    public NavigationController(Func<Page1ViewModel> page1ViewModelFactory, Func<Page2ViewModel> page2ViewModelFactory)
    {
        this.page1ViewModelFactory = page1ViewModelFactory ?? throw new ArgumentNullException(nameof(page1ViewModelFactory));
        this.page2ViewModelFactory = page2ViewModelFactory ?? throw new ArgumentNullException(nameof(page2ViewModelFactory));
    }

    public void NavigateToPage1()
    {
        this.Delegate?.NavigateTo(this.page1ViewModelFactory());
    }

    public void NavigateToPage2(string initiator)
    {
        Page2ViewModel vm = this.page2ViewModelFactory();
        vm.Initiator = initiator;
        this.Delegate?.NavigateTo(vm);
    }
}
