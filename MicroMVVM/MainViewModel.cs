using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroMVVM
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
