using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public interface IStyletIoCBindTo<TService>
    {
        void ToSelf(string key = null);
        void To<TImplementation>(string key = null);
        void ToFactory<TImplementation>(Func<StyletIoC, TImplementation> factory);
        void ToFactory<TImplementation>(string key, Func<StyletIoC, TImplementation> factory);
    }

    public class StyletIoC
    {
        #region Main Class

        private Dictionary<Type, List<IRegistration>> registrations = new Dictionary<Type, List<IRegistration>>();

        public IStyletIoCBindTo<TService> Bind<TService>()
        {
            return new BindTo<TService>(this, false);
        }

        public IStyletIoCBindTo<TService> BindSingleton<TService>()
        {
            return new BindTo<TService>(this, true);
        }

        public void Compile()
        {
            foreach (var registrations in this.registrations.Values)
            {
                foreach (var registration in registrations)
                    registration.EnsureGenerator(this);
            }
        }

        public object Get(Type type, string key = null)
        {
            return this.GetRegistration(type, key).Generator();
        }

        public T Get<T>(string key = null)
        {
            return (T)this.Get(typeof(T), key);
        }

        private bool CanResolve(Type type)
        {
            return this.registrations.ContainsKey(type);
        }

        private Expression GetExpression(Type type, string key)
        {
            return this.GetRegistration(type, key).GetInstanceExpression(this);
        }

        private List<IRegistration> GetRegistrations(Type type, string key)
        {
            if (!this.registrations.ContainsKey(type))
                throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0}.", type.Name));

            var registrations = this.registrations[type];
            if (key != null)
                registrations = registrations.Where(x => x.Key == key).ToList();

            if (registrations.Count == 0)
                throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0} with key '{1}'.", type.Name, key));

            return registrations;
        }

        private IRegistration GetRegistration(Type type, string key)
        {
            var registrations = this.GetRegistrations(type, key);

            if (registrations.Count > 1)
                throw new StyletIoCRegistrationException(String.Format("Multiple registrations found for service {0} with key '{1}'.", type.Name, key));

            return registrations[0];
        }

        private void AddRegistration(Type type, IRegistration registration)
        {
            if (!this.registrations.ContainsKey(type))
                this.registrations[type] = new List<IRegistration>();

            this.registrations[type].Add(registration);
        }

        #endregion

        #region BindTo

        private class BindTo<TService> : IStyletIoCBindTo<TService>
        {
            private StyletIoC service;
            private bool isSingleton;

            public BindTo(StyletIoC service, bool isSingleton)
            {
                this.service = service;
                this.isSingleton = isSingleton;
            }

            public void ToSelf(string key = null)
            {
                this.To<TService>(key);
            }

            public void To<TImplementation>(string key = null)
            {
                Type implementationType = typeof(TImplementation);
                this.EnsureType(implementationType);
                this.Add<TImplementation>(new TypeCreator(implementationType, key));
            }

            public void ToFactory<TImplementation>(string key, Func<StyletIoC, TImplementation> factory)
            {
                Type implementationType = typeof(TImplementation);
                this.EnsureType(implementationType);
                this.Add<TImplementation>(new FactoryCreator<TImplementation>(factory, key));
            }

            public void ToFactory<TImplementation>(Func<StyletIoC, TImplementation> factory)
            {
                this.ToFactory<TImplementation>(null, factory);
            }

            private void EnsureType(Type implementationType)
            {
                Type serviceType = typeof(TService);
                if (!serviceType.IsAssignableFrom(implementationType))
                    throw new StyletIoCException(String.Format("Type {0} does not implement service {1}", implementationType.Name, serviceType.Name));
            }

            private void Add<TImplementation>(ICreator creator)
            {
                Type serviceType = typeof(TService);
                Type implementationType = typeof(TImplementation);

                IRegistration registration;
                if (this.isSingleton)
                    registration = new SingletonRegistration<TImplementation>(creator);
                else
                    registration = new TransientRegistration(creator);

                service.AddRegistration(serviceType, registration);
            }
        }

        #endregion

        #region IRegistration

        private interface IRegistration
        {
            string Key { get; }
            Type Type { get; }
            Func<object> Generator { get; }
            void EnsureGenerator(StyletIoC service);
            Expression GetInstanceExpression(StyletIoC service);
        }

        private class TransientRegistration : IRegistration
        {
            public string Key { get { return this.creator.Key; } }
            public Type Type { get { return this.creator.Type; } }
            private ICreator creator;
            public Func<object> Generator { get; private set; }

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

            public string Key { get { return this.creator.Key; } }
            public Type Type { get { return this.creator.Type; } }
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

        #endregion

        #region ICreator

        private interface ICreator
        {
            string Key { get; }
            Type Type { get; }
            Expression GetInstanceExpression(StyletIoC service);
        }

        private class TypeCreator : ICreator
        {
            public string Key { get; private set; }
            public Type Type { get; private set; }
            private Expression creationExpression;

            public TypeCreator(Type type, string key = null)
            {
                this.Type = type;

                // Take the given key, but use the key from InjectAttribute (if present) if it's null
                if (key == null)
                {
                    var attribute = type.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault();
                    if (attribute != null)
                        key = ((InjectAttribute)attribute).Key;
                }
                this.Key = key;
            }

            public Expression GetInstanceExpression(StyletIoC service)
            {
                if (this.creationExpression != null)
                    return this.creationExpression;

                // Find the constructor which has the most parameters which we can fulfill, accepting default values which we can't fulfill
                ConstructorInfo ctor;
                var ctorsWithAttribute = this.Type.GetConstructors().Where(x => x.GetCustomAttributes(typeof(InjectAttribute), false).Any()).ToList();
                if (ctorsWithAttribute.Count > 1)
                {
                    throw new StyletIoCException(String.Format("Found more than one constructor with [Inject] on type {0}", this.Type.Name));
                }
                else if (ctorsWithAttribute.Count == 1)
                {
                    ctor = ctorsWithAttribute[0];
                    var cantResolve = ctor.GetParameters().Where(p => !service.CanResolve(p.ParameterType) && !p.HasDefaultValue).FirstOrDefault();
                    if (cantResolve != null)
                        throw new StyletIoCException(String.Format("Found a constructor with [Inject] on type {0}, but can't resolve parameter '{1}' (which doesn't have a default value).", this.Type.Name, cantResolve.Name));
                }
                else
                {
                    ctor = this.Type.GetConstructors()
                        .Where(c => c.GetParameters().All(p => service.CanResolve(p.ParameterType) || p.HasDefaultValue))
                        .OrderByDescending(c => c.GetParameters().Count(p => !p.HasDefaultValue))
                        .FirstOrDefault();

                    if (ctor == null)
                    {
                        throw new StyletIoCException(String.Format("Unable to find a constructor for type {0} which we can call", this.Type.Name));
                    }
                }

                // Check for loops

                // If there parameter's got an InjectAttribute with a key, use that key to resolve
                var ctorParams = ctor.GetParameters().Select(x =>
                {
                    if (service.CanResolve(x.ParameterType))
                    {
                        string key = null;
                        var attribute = x.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault();
                        if (attribute != null)
                            key = ((InjectAttribute)attribute).Key;
                        try
                        {
                            return service.GetExpression(x.ParameterType, key);
                        }
                        catch (StyletIoCRegistrationException e)
                        {
                            throw new StyletIoCRegistrationException(String.Format("{0} Required by paramter '{1}' of type {2}", e.Message, x.Name, this.Type.Name), e);
                        }
                    }
                    return Expression.Constant(x.DefaultValue);
                });

                var creator = Expression.New(ctor, ctorParams);
                this.creationExpression = creator;
                return creator;
            }
        }

        private class FactoryCreator<T> : ICreator
        {
            public string Key { get; private set; }
            public Type Type { get { return typeof(T); } }
            private Func<StyletIoC, T> factory;

            public FactoryCreator(Func<StyletIoC, T> factory, string key = null)
            {
                this.factory = factory;
                this.Key = key;
            }

            public Expression GetInstanceExpression(StyletIoC service)
            {
                var expr = (Expression<Func<T>>)(() => this.factory(service));
                return Expression.Invoke(expr, null);
            }
        }

        #endregion
    }

    public class StyletIoCException : Exception
    {
        public StyletIoCException(string message) : base(message) { }
        public StyletIoCException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class StyletIoCRegistrationException : StyletIoCException
    {
        public StyletIoCRegistrationException(string message) : base(message) { }
        public StyletIoCRegistrationException(string message, Exception innerException) : base(message, innerException) { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class InjectAttribute : Attribute
    {
        public InjectAttribute()
        {
        }

        public InjectAttribute(string key)
        {
            this.Key = key;
        }

        // This is a named argument
        public string Key { get; set; }
    }
}
