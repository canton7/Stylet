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

        /// <summary>
        /// Gets or sets a value indicating whether generated Expressions are cached, or resolved anew each time.
        /// Setting to true may speed up the compilation phase in the case where one type is required by many other types.
        /// However, it will cause loads of ConstructorInfos to be cached forever, and they're actually pretty expensive to hold on to.
        /// </summary>
        public static bool CacheGeneratedExpressions { get; set; }

        static StyletIoCContainer()
        {
            CacheGeneratedExpressions = false;
        }
    }
}
