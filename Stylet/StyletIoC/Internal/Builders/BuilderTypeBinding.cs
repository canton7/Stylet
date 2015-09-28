using System;
using System.Collections.Generic;
using StyletIoC.Creation;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderTypeBinding : BuilderBindingBase
    {
        private readonly Type implementationType;

        public BuilderTypeBinding(List<BuilderTypeKey> serviceTypes, Type implementationType)
            : base(serviceTypes)
        {
            this.EnsureTypeAgainstServiceTypes(implementationType);
            this.implementationType = implementationType;
        }

        public override void Build(Container container)
        {
            this.BindImplementationToServices(container, this.implementationType);
        }
    }
}
