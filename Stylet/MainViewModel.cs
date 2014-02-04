using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    class MainViewModel : Screen
    {
        public object ViewModel
        {
            get { return new SubViewModel(); }
        }

        protected override void OnViewLoaded()
        {
            this.TryClose();
        }
    }

    class SubViewModel
    {
        public string Testy { get { return "Testy"; } }
    }
}
