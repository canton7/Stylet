using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
