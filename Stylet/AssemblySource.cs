using System;
using System.Reflection;

namespace Stylet
{
    /// <summary>
    /// Container for the list of assemblies in which Stylet will look for types
    /// </summary>
    public static class AssemblySource
    {
        /// <summary>
        /// List of assemblies in which Stylet will look for types, (for autobinding in StyletIoC, and for finding Views).
        /// Populated by the Bootstrapper
        /// </summary>
        public static readonly IObservableCollection<Assembly> Assemblies = new BindableCollection<Assembly>();
    }
}
