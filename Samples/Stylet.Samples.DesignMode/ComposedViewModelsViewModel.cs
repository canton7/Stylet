using System;

namespace Stylet.Samples.DesignMode
{
    public class ComposedViewModelsViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public ComposedViewModelsViewModel()
        {
            this.ActivateItem(new Screen());
            this.ActivateItem(new Screen());
        }
    }
}
