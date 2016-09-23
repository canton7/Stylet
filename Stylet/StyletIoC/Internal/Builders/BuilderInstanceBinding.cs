using System;
using System.Collections.Generic;
using StyletIoC.Creation;
using StyletIoC.Internal.Creators;
using StyletIoC.Internal.Registrations;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderInstanceBinding : BuilderBindingBase, IWithKeyOrAsWeakBindingOrDisposeWithContainer
    {
        private readonly object instance;
        private bool disposeWithContainer = true;

        public BuilderInstanceBinding(List<BuilderTypeKey> serviceTypes, object instance)
            : base(serviceTypes)
        {
            this.EnsureTypeAgainstServiceTypes(instance.GetType(), assertImplementation: false);
            this.instance = instance;
        }

        public override void Build(Container container)
        {
            var registration = new InstanceRegistration(container, this.instance, this.disposeWithContainer);

            foreach (var serviceType in this.ServiceTypes)
            {
                container.AddRegistration(new TypeKey(serviceType.Type.TypeHandle, serviceType.Key), registration);
            }
        }

        public IWithKeyOrAsWeakBinding DisposeWithContainer(bool disposeWithContainer)
        {
            this.disposeWithContainer = disposeWithContainer;
            return this;
        }
    }
}
