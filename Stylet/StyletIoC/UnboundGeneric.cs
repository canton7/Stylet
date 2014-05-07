using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace StyletIoC
{
    internal class UnboundGeneric
    {
        private StyletIoCContainer container;
        public string Key { get; set; }
        public Type Type { get; private set; }
        public int NumTypeParams
        {
            get { return this.Type.GetTypeInfo().GenericTypeParameters.Length; }
        }
        public bool IsSingleton { get; private set; }

        public UnboundGeneric(Type type, StyletIoCContainer container, bool isSingleton)
        {
            this.Type = type;
            this.container = container;
            this.IsSingleton = isSingleton;
        }

        public IRegistration CreateRegistrationForType(Type boundType)
        {
            if (this.IsSingleton)
                return new SingletonRegistration(new TypeCreator(boundType, this.container));
            else
                return new TransientRegistration(new TypeCreator(boundType, this.container));
        }
    }
}
