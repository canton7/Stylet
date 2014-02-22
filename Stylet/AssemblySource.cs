using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public class AssemblySource
    {
        public static readonly IObservableCollection<Assembly> Assemblies = new BindableCollection<Assembly>();
    }
}
