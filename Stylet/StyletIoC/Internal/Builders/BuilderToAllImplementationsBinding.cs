using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderToAllImplementationsBinding : BuilderBindingBase
    {
        private readonly IEnumerable<Assembly> assemblies;

        public BuilderToAllImplementationsBinding(Type serviceType, IEnumerable<Assembly> assemblies)
            : base(serviceType)
        {
            this.assemblies = assemblies;
        }

        public override void Build(Container container)
        {
            var candidates = from type in this.assemblies.Distinct().SelectMany(x => x.GetTypes())
                             let baseType = type.GetBaseTypesAndInterfaces().FirstOrDefault(x => x == this.ServiceType || (x.IsGenericType && x.GetGenericTypeDefinition() == this.ServiceType))
                             where baseType != null
                             select new { Type = type, Base = baseType.ContainsGenericParameters ? baseType.GetGenericTypeDefinition() : baseType };

            foreach (var candidate in candidates)
            {
                try
                {
                    this.EnsureType(candidate.Type, candidate.Base);
                    this.BindImplementationToService(container, candidate.Type, candidate.Base);
                }
                catch (StyletIoCRegistrationException e)
                {
                    Debug.WriteLine(String.Format("Unable to auto-bind type {0} to {1}: {2}", candidate.Base.Name, candidate.Type.GetDescription(), e.Message), "StyletIoC");
                }
            }
        }
    }
}
