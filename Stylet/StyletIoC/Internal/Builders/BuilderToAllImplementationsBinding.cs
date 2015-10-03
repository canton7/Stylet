using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using StyletIoC.Creation;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderToAllImplementationsBinding : BuilderBindingBase
    {
        private readonly IEnumerable<Assembly> assemblies;
        private BuilderTypeKey ServiceType { get { return this.ServiceTypes[0]; } }

        public BuilderToAllImplementationsBinding(List<BuilderTypeKey> serviceTypes, IEnumerable<Assembly> assemblies)
            : base(serviceTypes)
        {
            // This should be ensured by the fluent interfaces
            Trace.Assert(this.ServiceTypes.Count == 1);

            this.assemblies = assemblies;
        }

        public override void Build(Container container)
        {
            var candidates = from type in this.assemblies.Distinct().SelectMany(x => x.GetTypes())
                             let baseType = type.GetBaseTypesAndInterfaces().FirstOrDefault(x => x == this.ServiceType.Type || (x.IsGenericType && x.GetGenericTypeDefinition() == this.ServiceType.Type))
                             where baseType != null
                             select new { Type = type, Base = baseType.ContainsGenericParameters ? baseType.GetGenericTypeDefinition() : baseType };

            foreach (var candidate in candidates)
            {
                try
                {
                    EnsureType(candidate.Type, candidate.Base);
                    this.BindImplementationToSpecificService(container, candidate.Type, candidate.Base, this.ServiceType.Key);
                }
                catch (StyletIoCRegistrationException e)
                {
                    Debug.WriteLine(String.Format("Unable to auto-bind type {0} to {1}: {2}", candidate.Base.Name, candidate.Type.GetDescription(), e.Message), "StyletIoC");
                }
            }
        }
    }
}
