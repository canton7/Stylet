using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC
{
    /// <summary>
    /// Lightweight, very fast IoC container
    /// </summary>
    public class StyletIoCContainer
    {
        /// <summary>
        /// Name of the assembly in which abstract factories are built. Use in [assembly: InternalsVisibleTo(StyletIoCContainer.FactoryAssemblyName)] to allow factories created by .ToAbstractFactory() to access internal types
        /// </summary>
        public static readonly string FactoryAssemblyName = "StyletIoCFactory";
    }
}
