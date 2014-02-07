using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public class StyletIoC
    {
        private Dictionary<Type, IRegistration> registrations = new Dictionary<Type, IRegistration>();

        public void Bind<TInterface, TImplementation>()
        {
            Type interfaceType = typeof(TInterface);
            Type implementationType = typeof(TImplementation);

            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new Exception(String.Format("Type {0} does not implement interface {1}", implementationType.Name, interfaceType.Name));

            this.registrations.Add(interfaceType, new TransientRegistration(new TypeCreator(implementationType)));
        }

        public void BindSingleton<TInterface, TImplementation>()
        {
            Type interfaceType = typeof(TInterface);
            Type implementationType = typeof(TImplementation);

            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new Exception(String.Format("Type {0} does not implement interface {1}", implementationType.Name, interfaceType.Name));

                this.registrations.Add(interfaceType, new SingletonRegistration<TImplementation>(new TypeCreator(implementationType)));
        }

        public void BindFactory<TInterface, TImplementation>(Func<StyletIoC, TImplementation> factory)
        {
            Type interfaceType = typeof(TInterface);
            Type implementationType = typeof(TImplementation);

            this.registrations.Add(interfaceType, new TransientRegistration(new FactoryCreator<TImplementation>(factory)));
        }

        public void Compile()
        {
            foreach (var registration in this.registrations.Values)
            {
                registration.EnsureGenerator(this);
            }
        }

        public object Get(Type type)
        {
            return this.registrations[type].Generator();
        }

        public T Get<T>()
        {
            return (T)this.Get(typeof(T));
        }

        private Expression GetExpression(Type type)
        {
            var registration = this.registrations[type];
            return registration.GetInstanceExpression(this);
        }


  
        private interface IRegistration
        {
            Func<object> Generator { get; }
            void EnsureGenerator(StyletIoC service);
            Expression GetInstanceExpression(StyletIoC service);
        }

        private class TransientRegistration : IRegistration
        {
            private ICreator creator;
            public Func<object> Generator { get; private set;}

            public TransientRegistration(ICreator creator)
            {
                this.creator = creator;
            }

            public Expression GetInstanceExpression(StyletIoC service)
            {
                return this.creator.GetInstanceExpression(service);
            }

            public void EnsureGenerator(StyletIoC service)
            {
                this.Generator = Expression.Lambda<Func<object>>(this.GetInstanceExpression(service)).Compile();
            }
        }

        private class SingletonRegistration<T> : IRegistration
        {
            private ICreator creator;
            private T instance;
            private Expression instanceExpression;
            public Func<object> Generator { get; private set; }


            public SingletonRegistration(ICreator creator)
            {
                this.creator = creator;
            }

            private void EnsureInstantiated(StyletIoC service)
            {
                if (this.instance == null)
                    this.instance = Expression.Lambda<Func<T>>(this.creator.GetInstanceExpression(service)).Compile()();
            }

            public void EnsureGenerator(StyletIoC service)
            {
                if (this.Generator != null)
                    return;

                this.EnsureInstantiated(service);
                this.Generator = () => this.instance;
            }

            public Expression GetInstanceExpression(StyletIoC service)
            {
                if (this.instanceExpression != null)
                    return this.instanceExpression;

                this.EnsureInstantiated(service);

                this.instanceExpression = Expression.Constant(this.instance);
                return this.instanceExpression;
            }
        }

        private interface ICreator
        {
            Expression GetInstanceExpression(StyletIoC service);
        }

        private class TypeCreator : ICreator
        {
            private Type type;
            private Expression creationExpression;

            public TypeCreator(Type type)
            {
                this.type = type;
            }

            public Expression GetInstanceExpression(StyletIoC service)
            {
                if (this.creationExpression != null)
                    return this.creationExpression;

                // Pick a suitable one
                var ctor = this.type.GetConstructors()[0];
                // Check for loops
                // Check for default values
                var ctorParams = ctor.GetParameters().Select(x => service.GetExpression(x.ParameterType));
                var creator = Expression.New(ctor, ctorParams);
                this.creationExpression = creator;
                return creator;
            }
        }

        private class FactoryCreator<T> : ICreator
        {
            private Func<StyletIoC, T> factory;

            public FactoryCreator(Func<StyletIoC, T> factory)
            {
                this.factory = factory;
            }

            public Expression GetInstanceExpression(StyletIoC service)
            {
                var expr = (Expression<Func<T>>)(() => this.factory(service));
                return expr;
            }
        }   
    }
}

