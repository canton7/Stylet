using System;
using System.Collections.Generic;
using StyletIoC.Creation;
using StyletIoC.Internal.Creators;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderInstanceBinding : BuilderBindingBase
    {
        private readonly object instance;

        public BuilderInstanceBinding(List<BuilderTypeKey> serviceTypes, object instance)
            : base(serviceTypes)
        {
            this.EnsureTypeAgainstServiceTypes(instance.GetType(), assertImplementation: false);
            this.instance = instance;
        }

        public override void Build(Container container)
        {
            var creator = new InstanceCreator(this.instance);
            var registration = this.CreateRegistration(container, creator);

            foreach (var serviceType in this.ServiceTypes)
            {
                container.AddRegistration(new TypeKey(serviceType.Type.TypeHandle, serviceType.Key), registration);
            }
        }
    }
}
