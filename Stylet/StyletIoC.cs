using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public interface IKernel
    {
        IStyletIoCBindTo Bind<TService>();
        IStyletIoCBindTo Bind(Type serviceType);
        IStyletIoCBindTo BindSingleton<TService>();
        IStyletIoCBindTo BindSingleton(Type serviceType);

        void AutoBind(params Assembly[] assemblies);

        void Compile();
        object Get(Type type, string key = null);
        T Get<T>(string key = null);
        IEnumerable<object> GetAll(Type type, string key = null);
        IEnumerable<T> GetAll<T>(string key = null);
    }

    public interface IStyletIoCBindTo
    {
        IStyletIoCBindWithKey ToSelf();
        IStyletIoCBindWithKey To<TImplementation>() where TImplementation : class;
        IStyletIoCBindWithKey To(Type implementationType);
        IStyletIoCBindWithKey ToFactory<TImplementation>(Func<IKernel, TImplementation> factory) where TImplementation : class;
        IStyletIoCBindWithKey ToAllImplementations(params Assembly[] assembly);
    }

    public interface IStyletIoCBindWithKey
    {
        void WithKey(string key);
    }

    public class StyletIoC : IKernel
    {
        #region Main Class

        private Dictionary<Type, List<IRegistration>> registrations = new Dictionary<Type, List<IRegistration>>();
        private Dictionary<Type, List<IRegistration>> getAllRegistrations = new Dictionary<Type, List<IRegistration>>();
        private Dictionary<Type, List<UnboundGeneric>> unboundGenerics = new Dictionary<Type, List<UnboundGeneric>>();

        private bool compilationStarted;

        public void AutoBind(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };

            var classes = assemblies.SelectMany(x => x.GetTypes()).Where(c => c.IsClass && !c.IsAbstract);
            foreach (var cls in classes)
            {
                try
                {
                    this.Bind(cls).To(cls);
                }
                catch (StyletIoCRegistrationException e)
                {
                    Debug.WriteLine(String.Format("Unable to auto-bind type {0}: {1}", cls.Name, e.Message), "StyletIoC");
                }
            }
        }

        public IStyletIoCBindTo Bind<TService>()
        {
            return this.Bind(typeof(TService));
        }

        public IStyletIoCBindTo Bind(Type serviceType)
        {
            this.CheckCompilationStarted();
            return new BindTo(this, serviceType, false);
        }

        public IStyletIoCBindTo BindSingleton<TService>()
        {
            return this.BindSingleton(typeof(TService));
        }

        public IStyletIoCBindTo BindSingleton(Type serviceType)
        {
            this.CheckCompilationStarted();
            return new BindTo(this, serviceType, true);
        }

        private void CheckCompilationStarted()
        {
            if (this.compilationStarted)
                throw new StyletIoCException("Once you've started to retrieve items from the container, or have called Compile(), you cannot register new services");
        }

        public void Compile()
        {
            this.compilationStarted = true;
            foreach (var kvp in this.registrations)
            {
                foreach (var registration in kvp.Value)
                {
                    try
                    {
                        registration.GetGenerator(this);
                    }
                    catch (StyletIoCFindConstructorException)
                    {
                        // If we can't resolve an auto-created type, that's fine
                        // Don't remove it from the list of types - that way they'll get a
                        // decent error message if they actually try and resolve it
                        if (!registration.WasAutoCreated)
                            throw;
                    }
                }
            }
        }

        public object Get(Type type, string key = null)
        {
            return this.GetRegistration(type, key, false).GetGenerator(this)();
        }

        public T Get<T>(string key = null)
        {

            return (T)this.Get(typeof(T), key);
        }

        public IEnumerable<object> GetAll(Type type, string key = null)
        {
            if (!this.TryEnsureGetAllRegistrationCreatedFromElementType(type, null, key))
                throw new StyletIoCRegistrationException(String.Format("Could not find registration for type {0} and key '{1}'", type.Name));
            return (IEnumerable<object>)this.getAllRegistrations[type].Single(x => x.Key == key).GetGenerator(this)();
        }

        public IEnumerable<T> GetAll<T>(string key = null)
        {
            return this.GetAll(typeof(T), key).Cast<T>();
        }

        private bool CanResolve(Type type, string key)
        {
            this.TryEnsureGenericRegistrationCreated(type, key);

            if (this.registrations.ContainsKey(type) && this.registrations[type].Any(x => x.Key == key))
                return true;

            // Is it a 'get all' request?
            var elementType = this.TryEnsureGetAllRegistrationCreated(type, key);
            return elementType != null;
        }

        private Type GetElementTypeFromCollectionType(Type type)
        {
            if (!type.IsGenericType || type.GenericTypeArguments.Length != 1 || !this.registrations.ContainsKey(type.GenericTypeArguments[0]))
                return null;
            return type.GenericTypeArguments[0];
        }

        private bool TryEnsureGetAllRegistrationCreatedFromElementType(Type elementType, Type collectionTypeOrNull, string key)
        {
            if (this.getAllRegistrations.ContainsKey(elementType) && this.getAllRegistrations[elementType].Any(x => x.Key == key))
                return true;

            var listType = typeof(List<>).MakeGenericType(elementType);
            if (collectionTypeOrNull != null && !collectionTypeOrNull.IsAssignableFrom(listType))
                return false;

            var registration = new GetAllRegistration(listType) { Key = key };
            this.AddGetAllRegistration(elementType, registration);
            return true;
        }

        // Returns the type of element if it's valid
        private Type TryEnsureGetAllRegistrationCreated(Type type, string key)
        {
            var elementType = this.GetElementTypeFromCollectionType(type);
            if (elementType == null)
                return null;

            return this.TryEnsureGetAllRegistrationCreatedFromElementType(elementType, type, key) ? elementType : null;
        }

        private void TryEnsureGenericRegistrationCreated(Type type, string key)
        {
            if (this.registrations.ContainsKey(type) || !type.IsGenericType || type.GenericTypeArguments.Length == 0)
                return;

            Type unboundGenericType = type.GetGenericTypeDefinition();

            if (!this.unboundGenerics.ContainsKey(unboundGenericType))
                return;

            var unboundGenerics = this.unboundGenerics[unboundGenericType].Where(x => x.Key == key);
            foreach (var unboundGeneric in unboundGenerics)
            {
                if (unboundGeneric == null)
                    break;

                // Consider this scenario:
                // interface IC<T, U> { } class C<T, U> : IC<U, T> { }
                // Then they ask for an IC<int, bool>. We need to give them a C<bool, int>
                // Search the ancestry of C for an IC (called implOfUnboundGenericType), then create a mapping which says that
                // U is a bool and T is an int by comparing this against 'type' - the IC<T, U> that's registered as the service
                // Then use this when making the type for C

                Type newType;
                if (unboundGeneric.Type == unboundGenericType)
                {
                    newType = type;
                }
                else
                {
                    var implOfUnboundGenericType = unboundGeneric.Type.GetBaseTypesAndInterfaces().Single(x => x.Name == unboundGenericType.Name);
                    var mapping = implOfUnboundGenericType.GenericTypeArguments.Zip(type.GenericTypeArguments, (n, t) => new { Type = t, Name = n });

                    newType = unboundGeneric.Type.MakeGenericType(unboundGeneric.Type.GetTypeInfo().GenericTypeParameters.Select(x => mapping.Single(t => t.Name.Name == x.Name).Type).ToArray());
                }

                if (!type.IsAssignableFrom(newType))
                    break;

                // Right! We've made a new generic type we can use
                var registration = unboundGeneric.CreateRegistrationForType(newType);
                this.AddRegistration(type, registration);
            }
        }

        private Expression GetExpression(Type type, string key, bool searchGetAllTypes)
        {
            return this.GetRegistration(type, key, searchGetAllTypes).GetInstanceExpression(this);
        }

        private IEnumerable<IRegistration> GetRegistrations(Type type, string key, bool searchGetAllTypes)
        {
            IEnumerable<IRegistration> registrations;

            this.TryEnsureGenericRegistrationCreated(type, key);

            if (!this.registrations.ContainsKey(type))
            {
                if (searchGetAllTypes)
                {
                    // Couldn't find this type - is it a 'get all' collection type? (i.e. they've put IEnumerable<TypeWeCanResolve> in a ctor param)
                    var collectionElementType = this.TryEnsureGetAllRegistrationCreated(type, key);
                    if (collectionElementType == null)
                        throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0}.", type.Name));

                    // Got this far? Good. There's actually a 'get all' collection type. Proceed with that
                    registrations = this.getAllRegistrations[collectionElementType];
                }
                else
                {
                    throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0}.", type.Name));
                }
            }
            else
            {
                registrations = this.registrations[type];
            }

            registrations = registrations.Where(x => x.Key == key);

            if (!registrations.Any())
                throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0} with key '{1}'.", type.Name, key));

            return registrations;
        }

        private IRegistration GetRegistration(Type type, string key, bool searchGetAllTypes)
        {
            IRegistration registration;

            var registrations = this.GetRegistrations(type, key, searchGetAllTypes).ToList();
            if (registrations.Count > 1)
                throw new StyletIoCRegistrationException(String.Format("Multiple registrations found for service {0} with key '{1}'.", type.Name, key));
            registration = registrations[0];

            return registration;
        }

        private void AddRegistration(Type type, IRegistration registration)
        {
            if (!this.registrations.ContainsKey(type))
                this.registrations[type] = new List<IRegistration>();

            // Is there an auto-registration for this type? If so, remove it
            var existingRegistration = this.registrations[type].Where(x => x.Key == registration.Key && x.Type == registration.Type).FirstOrDefault();
            if (existingRegistration != null)
            {
                if (existingRegistration.WasAutoCreated)
                    this.registrations[type].Remove(existingRegistration);
                else
                    throw new StyletIoCRegistrationException(String.Format("Multiple registrations for type {0} found", type.Name));
            }

            this.registrations[type].Add(registration);
        }

        private void AddUnboundGeneric(Type type, UnboundGeneric unboundGeneric)
        {
            if (!this.unboundGenerics.ContainsKey(type))
                this.unboundGenerics[type] = new List<UnboundGeneric>();

            // Is there an auto-registration for this type? If so, remove it
            var existingEntry = this.unboundGenerics[type].Where(x => x.Key == unboundGeneric.Key && x.Type == unboundGeneric.Type).FirstOrDefault();
            if (existingEntry != null)
            {
                if (existingEntry.WasAutoCreated)
                    this.unboundGenerics[type].Remove(existingEntry);
                else
                    throw new StyletIoCRegistrationException(String.Format("Multiple registrations for type {0} found", type.Name));
            }

            this.unboundGenerics[type].Add(unboundGeneric);
        }

        private void AddGetAllRegistration(Type type, IRegistration registration)
        {
            if (!this.getAllRegistrations.ContainsKey(type))
                this.getAllRegistrations[type] = new List<IRegistration>();

            this.getAllRegistrations[type].Add(registration);
        }

        #endregion

        #region BindTo

        private class BindTo : IStyletIoCBindTo
        {
            private StyletIoC service;
            private Type serviceType;
            private bool isSingleton;

            public BindTo(StyletIoC service, Type serviceType, bool isSingleton)
            {
                this.service = service;
                this.serviceType = serviceType;
                this.isSingleton = isSingleton;
            }

            public IStyletIoCBindWithKey ToSelf()
            {
                return this.To(this.serviceType);
            }

            public IStyletIoCBindWithKey To<TImplementation>() where TImplementation : class
            {
                return this.To(typeof(TImplementation));
            }

            public IStyletIoCBindWithKey To(Type implementationType)
            {
                this.EnsureType(implementationType);
                IHasKey hasKey;
                if (this.serviceType.IsGenericTypeDefinition)
                {
                    var unboundGeneric = new UnboundGeneric(implementationType, this.isSingleton);
                    this.service.AddUnboundGeneric(serviceType, unboundGeneric);
                    hasKey = unboundGeneric;
                }
                else
                {
                    var creator = new TypeCreator(implementationType);
                    this.AddRegistration(creator, implementationType);
                    hasKey = creator;
                }
                return new BindToWithKey(hasKey);
            }

            public IStyletIoCBindWithKey ToFactory<TImplementation>(Func<IKernel, TImplementation> factory) where TImplementation : class
            {
                Type implementationType = typeof(TImplementation);
                this.EnsureType(implementationType);
                if (this.serviceType.IsGenericTypeDefinition)
                    throw new StyletIoCRegistrationException(String.Format("A factory cannot be used to implement unbound generic type {0}", this.serviceType.Name));
                var creator = new FactoryCreator<TImplementation>(factory);
                this.AddRegistration(creator, implementationType);
                return new BindToWithKey(creator);
            }

            public IStyletIoCBindWithKey ToAllImplementations(params Assembly[] assemblies)
            {
                if (assemblies == null || assemblies.Length == 0)
                    assemblies = new[] { Assembly.GetCallingAssembly() };

                var candidates = from type in assemblies.SelectMany(x => x.GetTypes())
                                 let baseType = type.GetBaseTypesAndInterfaces().FirstOrDefault(x => x == this.serviceType || x.IsGenericType && x.GetGenericTypeDefinition() == this.serviceType)
                                 where baseType != null
                                 select new { Type = type, Base = baseType.ContainsGenericParameters ? baseType.GetGenericTypeDefinition() : baseType };

                IEnumerable<IStyletIoCBindWithKey> haveKeys = candidates.Select(candidate =>
                {
                    try
                    {
                        return this.service.Bind(candidate.Base).To(candidate.Type);
                    }
                    catch (StyletIoCRegistrationException e)
                    {
                        Debug.WriteLine(String.Format("Unable to auto-bind type {0} to {1}: {2}", candidate.Base.Name, candidate.Type.Name, e.Message), "StyletIoC");
                        return null;
                    }
                });
                return new BindToWithKey(haveKeys.Where(x => x != null).ToArray());
            }

            private void EnsureType(Type implementationType)
            {
                if (!implementationType.IsClass || implementationType.IsAbstract)
                    throw new StyletIoCRegistrationException(String.Format("Type {0} is not a concrete class, and so can't be used to implemented service {1}", implementationType.Name, this.serviceType.Name));

                // Test this first, as it's a bit clearer than hitting 'type doesn't implement service'
                if (implementationType.IsGenericTypeDefinition)
                {
                    if (this.isSingleton)
                        throw new StyletIoCRegistrationException(String.Format("You cannot create singleton registration for unbound generic type {0}", implementationType.Name));

                    if (!this.serviceType.IsGenericTypeDefinition)
                        throw new StyletIoCRegistrationException(String.Format("You may not bind the unbound generic type {0} to the bound generic / non-generic service {1}", implementationType.Name, this.serviceType.Name));

                    // This restriction may change when I figure out how to pass down the correct type argument
                    if (this.serviceType.GetTypeInfo().GenericTypeParameters.Length != implementationType.GetTypeInfo().GenericTypeParameters.Length)
                        throw new StyletIoCRegistrationException(String.Format("If you're registering an unbound generic type to an unbound generic service, both service and type must have the same number of type parameters. Service: {0}, Type: {1}", this.serviceType.Name, implementationType.Name));
                }
                else if (this.serviceType.IsGenericTypeDefinition)
                {
                    throw new StyletIoCRegistrationException(String.Format("You cannot bind the bound generic / non-generic type {0} to unbound generic service {1}", implementationType.Name, this.serviceType.Name));
                }

                if (!implementationType.Implements(this.serviceType))
                    throw new StyletIoCRegistrationException(String.Format("Type {0} does not implement service {1}", implementationType.Name, this.serviceType.Name));
            }

            private void AddRegistration(ICreator creator, Type implementationType)
            {
                IRegistration registration;
                if (this.isSingleton)
                    registration = new SingletonRegistration(creator);
                else
                    registration = new TransientRegistration(creator);

                 service.AddRegistration(this.serviceType, registration);
            }
        }

        private class BindToWithKey : IStyletIoCBindWithKey
        {
            private IHasKey hasKey;
            private IStyletIoCBindWithKey[] others;

            public BindToWithKey(IHasKey hasKey)
            {
                this.hasKey = hasKey;
                this.others = new IStyletIoCBindWithKey[0];
            }

            public BindToWithKey(params IStyletIoCBindWithKey[] others)
            {
                this.hasKey = null;
                this.others = others;
            }

            public void WithKey(string key)
            {
                if (this.hasKey != null)
                    this.hasKey.Key = key;

                foreach (var other in this.others)
                    other.WithKey(key);
            }
        }

        #endregion

        #region IHasKey

        private interface IHasKey
        {
            string Key { get; set; }
        }

        #endregion

        #region IRegistration

        private interface IRegistration : IHasKey
        {
            Type Type { get; }
            bool WasAutoCreated { get; set; }
            Func<object> GetGenerator(StyletIoC service);
            Expression GetInstanceExpression(StyletIoC service);
        }

        private abstract class RegistrationBase : IRegistration
        {
            protected ICreator creator;

            public string Key
            {
                get { return this.creator.Key; }
                set { this.creator.Key = value; }
            }
            public Type Type { get { return this.creator.Type; } }
            public bool WasAutoCreated { get; set; }

            protected Func<object> generator { get; set; }

            public abstract Func<object> GetGenerator(StyletIoC service);
            public abstract Expression GetInstanceExpression(StyletIoC service);
        }


        private class TransientRegistration : RegistrationBase
        {
            public TransientRegistration(ICreator creator)
            {
                this.creator = creator;
            }

            public override Expression GetInstanceExpression(StyletIoC service)
            {
                return this.creator.GetInstanceExpression(service);
            }

            public override Func<object> GetGenerator(StyletIoC service)
            {
                if (this.generator == null)
                    this.generator = Expression.Lambda<Func<object>>(this.GetInstanceExpression(service)).Compile();
                return this.generator;
            }
        }

        private class SingletonRegistration : RegistrationBase
        {
            private bool instanceInstantiated;
            private object instance;
            private Expression instanceExpression;

            public SingletonRegistration(ICreator creator)
            {
                this.creator = creator;
            }

            private void EnsureInstantiated(StyletIoC service)
            {
                if (this.instanceInstantiated)
                    return;

                this.instance = Expression.Lambda<Func<object>>(this.creator.GetInstanceExpression(service)).Compile()();
                this.instanceInstantiated = true;
            }

            public override Func<object> GetGenerator(StyletIoC service)
            {
                this.EnsureInstantiated(service);

                if (this.generator == null)
                    this.generator = () => this.instance;

                return this.generator;
            }

            public override Expression GetInstanceExpression(StyletIoC service)
            {
                if (this.instanceExpression != null)
                    return this.instanceExpression;

                this.EnsureInstantiated(service);

                // This expression yields the actual type of instance, not 'object'
                this.instanceExpression = Expression.Constant(this.instance);
                return this.instanceExpression;
            }
        }

        private class GetAllRegistration : IRegistration
        {
            public string Key { get; set; }
            public Type Type { get; private set; }
            public bool WasAutoCreated { get; set; }

            private Expression expression;
            private Func<object> generator;

            public GetAllRegistration(Type type)
            {
                this.Type = type;
            }

            public Func<object> GetGenerator(StyletIoC service)
            {
                if (this.generator == null)
                    this.generator = Expression.Lambda<Func<object>>(this.GetInstanceExpression(service)).Compile();
                return this.generator;
            }

            public Expression GetInstanceExpression(StyletIoC service)
            {
                if (this.expression != null)
                    return this.expression;

                var list = Expression.New(this.Type);
                var init = Expression.ListInit(list, service.GetRegistrations(this.Type.GenericTypeArguments[0], this.Key, false).Select(x => x.GetInstanceExpression(service)));
                
                this.expression = init;
                return init;
            }
        }

        #endregion

        #region ICreator

        private interface ICreator : IHasKey
        {
            Type Type { get; }
            Expression GetInstanceExpression(StyletIoC service);
        }

        private abstract class CreatorBase : ICreator
        {
            public string Key { get; set; }
            public virtual Type Type { get; protected set; }
            public abstract Expression GetInstanceExpression(StyletIoC service);
        }

        private class TypeCreator : CreatorBase
        {
            private Expression creationExpression;

            public TypeCreator(Type type)
            {
                this.Type = type;

                // Use the key from InjectAttribute (if present), and let someone else override it if they want
                var attribute = (InjectAttribute)type.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault();
                if (attribute != null)
                    this.Key = attribute.Key;
            }

            private string KeyForParameter(ParameterInfo parameter)
            {
                var attribute = (InjectAttribute)parameter.GetCustomAttributes(typeof(InjectAttribute)).FirstOrDefault();
                return attribute == null ? null : attribute.Key;
            }

            public override Expression GetInstanceExpression(StyletIoC service)
            {
                if (this.creationExpression != null)
                    return this.creationExpression;

                // Find the constructor which has the most parameters which we can fulfill, accepting default values which we can't fulfill
                ConstructorInfo ctor;
                var ctorsWithAttribute = this.Type.GetConstructors().Where(x => x.GetCustomAttributes(typeof(InjectAttribute), false).Any()).ToList();
                if (ctorsWithAttribute.Count > 1)
                {
                    throw new StyletIoCFindConstructorException(String.Format("Found more than one constructor with [Inject] on type {0}.", this.Type.Name));
                }
                else if (ctorsWithAttribute.Count == 1)
                {
                    ctor = ctorsWithAttribute[0];
                    var key = ((InjectAttribute)ctorsWithAttribute[0].GetCustomAttribute(typeof(InjectAttribute), false)).Key;
                    var cantResolve = ctor.GetParameters().Where(p => !service.CanResolve(p.ParameterType, key) && !p.HasDefaultValue).FirstOrDefault();
                    if (cantResolve != null)
                        throw new StyletIoCFindConstructorException(String.Format("Found a constructor with [Inject] on type {0}, but can't resolve parameter '{1}' (which doesn't have a default value).", this.Type.Name, cantResolve.Name));
                }
                else
                {
                    ctor = this.Type.GetConstructors()
                        .Where(c => c.GetParameters().All(p => service.CanResolve(p.ParameterType, this.KeyForParameter(p)) || p.HasDefaultValue))
                        .OrderByDescending(c => c.GetParameters().Count(p => !p.HasDefaultValue))
                        .FirstOrDefault();

                    if (ctor == null)
                    {
                        throw new StyletIoCFindConstructorException(String.Format("Unable to find a constructor for type {0} which we can call.", this.Type.Name));
                    }
                }

                // TODO: Check for loops

                // If there parameter's got an InjectAttribute with a key, use that key to resolve
                var ctorParams = ctor.GetParameters().Select(x =>
                {
                    var key = this.KeyForParameter(x);
                    if (service.CanResolve(x.ParameterType, key))
                    {
                        try
                        {
                            return service.GetExpression(x.ParameterType, key, true);
                        }
                        catch (StyletIoCRegistrationException e)
                        {
                            throw new StyletIoCRegistrationException(String.Format("{0} Required by paramter '{1}' of type {2}.", e.Message, x.Name, this.Type.Name), e);
                        }
                    }
                    // For some reason we need this cast...
                    return Expression.Convert(Expression.Constant(x.DefaultValue), x.ParameterType);
                });

                var creator = Expression.New(ctor, ctorParams);
                this.creationExpression = creator;
                return creator;
            }
        }

        private class FactoryCreator<T> : CreatorBase
        {
            public override Type Type { get { return typeof(T); } }
            private Func<StyletIoC, T> factory;

            public FactoryCreator(Func<StyletIoC, T> factory)
            {
                this.factory = factory;
            }

            public override Expression GetInstanceExpression(StyletIoC service)
            {
                var expr = (Expression<Func<T>>)(() => this.factory(service));
                return Expression.Invoke(expr, null);
            }
        }

        #endregion

        #region UnboundGeneric stuff

        private class UnboundGeneric : IHasKey
        {
            public bool WasAutoCreated { get; set; }
            public string Key { get; set; }
            public Type Type { get; private set; }
            public int NumTypeParams
            {
                get { return IntrospectionExtensions.GetTypeInfo(this.Type).GenericTypeParameters.Length; }
            }
            public bool IsSingleton { get; private set; }

            public UnboundGeneric(Type type, bool isSingleton)
            {
                this.Type = type;
            }

            public IRegistration CreateRegistrationForType(Type boundType)
            {
                if (this.IsSingleton)
                    return new SingletonRegistration(new TypeCreator(boundType)) { WasAutoCreated = this.WasAutoCreated, Key = this.Key };
                else
                    return new TransientRegistration(new TypeCreator(boundType)) { WasAutoCreated = this.WasAutoCreated, Key = this.Key };
            }
        }

        #endregion

    }

    internal static class TypeExtensions
    {
        public static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            return type.GetInterfaces().Concat(type.GetBaseTypes());
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            if (type == typeof(object))
                yield break;
            var baseType = type.BaseType ?? typeof(object);

            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        public static bool Implements(this Type implementationType, Type serviceType)
        {
            return serviceType.IsAssignableFrom(implementationType) ||
                implementationType.GetBaseTypesAndInterfaces().Any(x => x == serviceType || (x.IsGenericType && x.GetGenericTypeDefinition() == serviceType));
        }
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

    public class StyletIoCFindConstructorException : StyletIoCException
    {
        public StyletIoCFindConstructorException(string message) : base(message) { }
        public StyletIoCFindConstructorException(string message, Exception innerException) : base(message, innerException) { }
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
