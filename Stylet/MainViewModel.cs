using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    class MainViewModel
    {
        public object ViewModel
        {
            get { return new SubViewModel(); }
        }
    }

    class SubViewModel
    {

    }
}
