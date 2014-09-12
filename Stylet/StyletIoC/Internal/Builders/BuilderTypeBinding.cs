using System;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderTypeBinding : BuilderBindingBase
    {
        private Type implementationType;

        public BuilderTypeBinding(Type serviceType, Type implementationType)
            : base(serviceType)
        {
            this.EnsureType(implementationType);
            this.implementationType = implementationType;
        }

        public override void Build(Container container)
        {
            this.BindImplementationToService(container, this.implementationType);
        }
    }
}
