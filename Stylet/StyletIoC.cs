using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
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
        void ToSelf(string key = null);
        void To<TImplementation>(string key = null) where TImplementation : class;
        void To(Type implementationType, string key = null);
        void ToFactory<TImplementation>(Func<IKernel, TImplementation> factory, string key = null) where TImplementation : class;
        void ToAllImplementations(string key = null, params Assembly[] assembly);
    }

    public interface IStyletIoCBindWithKey
    {
        void WithKey(string key);
    }

    public class StyletIoC : IKernel
    {
        #region Main Class

        private ConcurrentDictionary<TypeKey, IRegistrationCollection> registrations = new ConcurrentDictionary<TypeKey, IRegistrationCollection>();
        private ConcurrentDictionary<TypeKey, IRegistration> getAllRegistrations = new ConcurrentDictionary<TypeKey, IRegistration>();
        private ConcurrentDictionary<TypeKey, List<UnboundGeneric>> unboundGenerics = new ConcurrentDictionary<TypeKey, List<UnboundGeneric>>();

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
                foreach (var registration in kvp.Value.GetAll())
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
            if (type == null)
                throw new ArgumentNullException("type");
            Func<object> generator;
            var registrations = this.GetRegistrations(new TypeKey(type, key), false);
            var registration = registrations.GetSingle();
            generator = registration.GetGenerator(this);
            return generator();
        }

        public T Get<T>(string key = null)
        {
            return (T)this.Get(typeof(T), key);
        }

        public IEnumerable<object> GetAll(Type type, string key = null)
        {
            var typeKey = new TypeKey(type, key);
            IRegistration registration;
            if (!this.TryEnsureGetAllRegistrationCreatedFromElementType(typeKey, null, out registration))
                throw new StyletIoCRegistrationException(String.Format("Could not find registration for type {0} and key '{1}'", typeKey.Type.Name));
            var generator = registration.GetGenerator(this);
            return (IEnumerable<object>)generator();
        }

        public IEnumerable<T> GetAll<T>(string key = null)
        {
            return this.GetAll(typeof(T), key).Cast<T>();
        }

        private bool CanResolve(TypeKey typeKey)
        {
            IRegistrationCollection registrations;

            if (this.registrations.TryGetValue(typeKey, out registrations) ||
                this.TryCreateGenericTypesForUnboundGeneric(typeKey, out registrations))
            {
                return true;
            }

            // Is it a 'get all' request?
            IRegistration registration;
            return this.TryEnsureGetAllRegistrationCreated(typeKey, out registration);
        }

        private Type GetElementTypeFromCollectionType(TypeKey typeKey)
        {
            Type type = typeKey.Type;
            // Elements are never removed from this.registrations, so we're safe to make this ContainsKey query
            if (!type.IsGenericType || type.GenericTypeArguments.Length != 1 || !this.registrations.ContainsKey(new TypeKey(type.GenericTypeArguments[0], typeKey.Key)))
                return null;
            return type.GenericTypeArguments[0];
        }

        private bool TryEnsureGetAllRegistrationCreatedFromElementType(TypeKey elementTypeKey, Type collectionTypeOrNull, out IRegistration registration)
        {
            // TryGet first, as making the generic type is expensive
            // If it isn't present, and can be made, GetOrAdd to try and add it, but return the now-existing registration if someone beat us to it
            if (this.getAllRegistrations.TryGetValue(elementTypeKey, out registration))
                return true;

            var listType = typeof(List<>).MakeGenericType(elementTypeKey.Type);
            if (collectionTypeOrNull != null && !collectionTypeOrNull.IsAssignableFrom(listType))
                return false;

            registration = this.getAllRegistrations.GetOrAdd(elementTypeKey, x => new GetAllRegistration(listType) { Key = elementTypeKey.Key });
            return true;
        }

        // Returns the type of element if it's valid
        private bool TryEnsureGetAllRegistrationCreated(TypeKey typeKey, out IRegistration registration)
        {
            registration = null;
            var elementType = this.GetElementTypeFromCollectionType(typeKey);
            if (elementType == null)
                return false;

            return this.TryEnsureGetAllRegistrationCreatedFromElementType(new TypeKey(elementType, typeKey.Key), typeKey.Type, out registration);
        }

        private bool TryCreateGenericTypesForUnboundGeneric(TypeKey typeKey, out IRegistrationCollection registrations)
        {
            registrations = null;
            var type = typeKey.Type;

            if (!type.IsGenericType || type.GenericTypeArguments.Length == 0)
                return false;

            Type unboundGenericType = type.GetGenericTypeDefinition();
            
            List<UnboundGeneric> unboundGenerics;
            if (!this.unboundGenerics.TryGetValue(new TypeKey(unboundGenericType, typeKey.Key), out unboundGenerics))
                return false;

            // Need to lock this, as someone might modify the underying list by registering a new unbound generic
            lock (unboundGenerics)
            {
                foreach (var unboundGeneric in unboundGenerics)
                {
                    if (unboundGeneric == null)
                        continue;

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
                        continue;

                    // Right! We've made a new generic type we can use
                    var registration = unboundGeneric.CreateRegistrationForType(newType);

                    // AddRegistration returns the IRegistrationCollection which was added/updated, so the one returned from the final
                    // call to AddRegistration is the final IRegistrationCollection for this key
                    registrations = this.AddRegistration(typeKey, registration);
                }
            }

            return registrations != null;
        }

        private Expression GetExpression(TypeKey typeKey, bool searchGetAllTypes)
        {
            return this.GetRegistrations(typeKey, searchGetAllTypes).GetSingle().GetInstanceExpression(this);
        }

        private IRegistrationCollection GetRegistrations(TypeKey typeKey, bool searchGetAllTypes)
        {
            IRegistrationCollection registrations;

            // Try to get registrations. If there are none, see if we can add some from unbound generics
            // If we still fail, try searching the 'get all' types
            if (!this.registrations.TryGetValue(typeKey, out registrations) &&
                !this.TryCreateGenericTypesForUnboundGeneric(typeKey, out registrations))
            {
                if (searchGetAllTypes)
                {
                    // Couldn't find this type - is it a 'get all' collection type? (i.e. they've put IEnumerable<TypeWeCanResolve> in a ctor param)
                    IRegistration registration;
                    if (!this.TryEnsureGetAllRegistrationCreated(typeKey, out registration))
                        throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0}.", typeKey.Type.Name));

                    // Got this far? Good. There's actually a 'get all' collection type. Proceed with that
                    registrations = new SingleRegistration(registration);
                }
                else
                {
                    throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0}.", typeKey.Type.Name));
                }
            }

            return registrations;
        }

        private IRegistrationCollection AddRegistration(TypeKey typeKey, IRegistration registration)
        {
            return this.registrations.AddOrUpdate(typeKey, x => new SingleRegistration(registration), (x, c) => c.AddRegistration(registration));
        }

        private void AddUnboundGeneric(TypeKey typeKey, UnboundGeneric unboundGeneric)
        {
            // We're not worried about thread-safety across multiple calls to this function (as it's only called as part of setup, which we're
            // not thread-safe about). However someone might be fetching something from this list while we're modifying it, which we need to avoid
            var unboundGenerics = this.unboundGenerics.GetOrAdd(typeKey, x => new List<UnboundGeneric>());
            lock (unboundGenerics)
            {
                // Is there an auto-registration for this type? If so, remove it
                var existingEntry = unboundGenerics.Where(x => x.Type == unboundGeneric.Type).FirstOrDefault();
                if (existingEntry != null)
                {
                    if (existingEntry.WasAutoCreated)
                        unboundGenerics.Remove(existingEntry);
                    else
                        throw new StyletIoCRegistrationException(String.Format("Multiple registrations for type {0} found", typeKey.Type.Name));
                }

                unboundGenerics.Add(unboundGeneric);
            }
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

            public void ToSelf(string key = null)
            {
                this.To(this.serviceType, key);
            }

            public void To<TImplementation>(string key = null) where TImplementation : class
            {
                this.To(typeof(TImplementation), key);
            }

            public void To(Type implementationType, string key = null)
            {
                this.EnsureType(implementationType);
                if (this.serviceType.IsGenericTypeDefinition)
                {
                    var unboundGeneric = new UnboundGeneric(implementationType, this.isSingleton);
                    this.service.AddUnboundGeneric(new TypeKey(serviceType, key), unboundGeneric);
                }
                else
                {
                    var creator = new TypeCreator(implementationType);
                    this.AddRegistration(creator, implementationType, key ?? creator.AttributeKey);
                }
            }

            public void ToFactory<TImplementation>(Func<IKernel, TImplementation> factory, string key = null) where TImplementation : class
            {
                Type implementationType = typeof(TImplementation);
                this.EnsureType(implementationType);
                if (this.serviceType.IsGenericTypeDefinition)
                    throw new StyletIoCRegistrationException(String.Format("A factory cannot be used to implement unbound generic type {0}", this.serviceType.Name));
                var creator = new FactoryCreator<TImplementation>(factory);
                this.AddRegistration(creator, implementationType, key);
            }

            public void ToAllImplementations(string key = null, params Assembly[] assemblies)
            {
                if (assemblies == null || assemblies.Length == 0)
                    assemblies = new[] { Assembly.GetCallingAssembly() };

                var candidates = from type in assemblies.SelectMany(x => x.GetTypes())
                                 let baseType = type.GetBaseTypesAndInterfaces().FirstOrDefault(x => x == this.serviceType || x.IsGenericType && x.GetGenericTypeDefinition() == this.serviceType)
                                 where baseType != null
                                 select new { Type = type, Base = baseType.ContainsGenericParameters ? baseType.GetGenericTypeDefinition() : baseType };

                foreach (var candidate in candidates)
                {
                    try
                    {
                        this.service.Bind(candidate.Base).To(candidate.Type, key);
                    }
                    catch (StyletIoCRegistrationException e)
                    {
                        Debug.WriteLine(String.Format("Unable to auto-bind type {0} to {1}: {2}", candidate.Base.Name, candidate.Type.Name, e.Message), "StyletIoC");
                    }
                }
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

            private void AddRegistration(ICreator creator, Type implementationType, string key)
            {
                IRegistration registration;
                if (this.isSingleton)
                    registration = new SingletonRegistration(creator);
                else
                    registration = new TransientRegistration(creator);

                service.AddRegistration(new TypeKey(this.serviceType, key), registration);
            }
        }

        #endregion

        #region IRegistration

        private interface IRegistration
        {
            Type Type { get; }
            bool WasAutoCreated { get; set; }
            Func<object> GetGenerator(StyletIoC container);
            Expression GetInstanceExpression(StyletIoC container);
        }

        private abstract class RegistrationBase : IRegistration
        {
            protected ICreator creator;

            public Type Type { get { return this.creator.Type; } }
            public bool WasAutoCreated { get; set; }

            protected Func<object> generator { get; set; }

            public abstract Func<object> GetGenerator(StyletIoC container);
            public abstract Expression GetInstanceExpression(StyletIoC container);
        }


        private class TransientRegistration : RegistrationBase
        {
            public TransientRegistration(ICreator creator)
            {
                this.creator = creator;
            }

            public override Expression GetInstanceExpression(StyletIoC container)
            {
                return this.creator.GetInstanceExpression(container);
            }

            public override Func<object> GetGenerator(StyletIoC container)
            {
                if (this.generator == null)
                    this.generator = Expression.Lambda<Func<object>>(this.GetInstanceExpression(container)).Compile();
                return this.generator;
            }
        }

        private class SingletonRegistration : RegistrationBase
        {
            private object instance;
            private Expression instanceExpression;

            public SingletonRegistration(ICreator creator)
            {
                this.creator = creator;
            }

            private void EnsureInstantiated(StyletIoC container)
            {
                if (this.instance != null)
                    return;

                // Ensure we don't end up creating two singletons, one used by each thread
                Interlocked.CompareExchange(ref this.instance, Expression.Lambda<Func<object>>(this.creator.GetInstanceExpression(container)).Compile()(), null);
            }

            public override Func<object> GetGenerator(StyletIoC container)
            {
                this.EnsureInstantiated(container);

                if (this.generator == null)
                    this.generator = () => this.instance;

                return this.generator;
            }

            public override Expression GetInstanceExpression(StyletIoC container)
            {
                if (this.instanceExpression != null)
                    return this.instanceExpression;

                this.EnsureInstantiated(container);

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

            public Func<object> GetGenerator(StyletIoC container)
            {
                if (this.generator == null)
                    this.generator = Expression.Lambda<Func<object>>(this.GetInstanceExpression(container)).Compile();
                return this.generator;
            }

            public Expression GetInstanceExpression(StyletIoC container)
            {
                if (this.expression != null)
                    return this.expression;

                var list = Expression.New(this.Type);
                var init = Expression.ListInit(list, container.GetRegistrations(new TypeKey(this.Type.GenericTypeArguments[0], this.Key), false).GetAll().Select(x => x.GetInstanceExpression(container)));
                
                this.expression = init;
                return init;
            }
        }

        #endregion

        private interface IRegistrationCollection
        {
            IRegistration GetSingle();
            List<IRegistration> GetAll();
            IRegistrationCollection AddRegistration(IRegistration registration);
        }

        private class SingleRegistration : IRegistrationCollection
        {
            private IRegistration registration;

            public SingleRegistration(IRegistration registration)
            {
                this.registration = registration;
            }

            public IRegistration GetSingle()
            {
                return this.registration;
            }

            public List<IRegistration> GetAll()
            {
                return new List<IRegistration>() { this.registration };
            }

            public IRegistrationCollection AddRegistration(IRegistration registration)
            {
                return new RegistrationCollection(new List<IRegistration>() { this.registration, registration });
            }
        }

        private class RegistrationCollection : IRegistrationCollection
        {
            private List<IRegistration> registrations;

            public RegistrationCollection(List<IRegistration> registrations)
            {
                this.registrations = registrations;
            }

            public IRegistration GetSingle()
            {
                throw new StyletIoCRegistrationException("Multiple registrations found.");
            }

            public List<IRegistration> GetAll()
            {
                List<IRegistration> registrationsCopy;
                lock (this.registrations) { registrationsCopy = registrations.ToList(); }
                return registrationsCopy;
            }

            public IRegistrationCollection AddRegistration(IRegistration registration)
            {
                // Need to lock the list, as someone might be fetching from it while we do this
                lock (this.registrations)
                {
                    // Is there an existing registration for this type?
                    var existingRegistration = this.registrations.FirstOrDefault(x => x.Type == registration.Type);
                    if (existingRegistration != null)
                    {
                        if (existingRegistration.WasAutoCreated)
                            this.registrations.Remove(existingRegistration);
                        else
                            throw new StyletIoCRegistrationException(String.Format("Multiple registrations for type {0} found.", registration.Type.Name));
                    }
                    this.registrations.Add(registration);
                    return this;
                }
            }
        }

        #region ICreator

        private interface ICreator
        {
            Type Type { get; }
            Expression GetInstanceExpression(StyletIoC container);
        }

        private abstract class CreatorBase : ICreator
        {
            public virtual Type Type { get; protected set; }
            public abstract Expression GetInstanceExpression(StyletIoC container);
        }

        private class TypeCreator : CreatorBase
        {
            public string AttributeKey { get; private set; }
            private Expression creationExpression;

            public TypeCreator(Type type)
            {
                this.Type = type;

                // Use the key from InjectAttribute (if present), and let someone else override it if they want
                var attribute = (InjectAttribute)type.GetCustomAttributes(typeof(InjectAttribute), false).FirstOrDefault();
                if (attribute != null)
                    this.AttributeKey = attribute.Key;
            }

            private string KeyForParameter(ParameterInfo parameter)
            {
                var attribute = (InjectAttribute)parameter.GetCustomAttributes(typeof(InjectAttribute)).FirstOrDefault();
                return attribute == null ? null : attribute.Key;
            }

            public override Expression GetInstanceExpression(StyletIoC container)
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
                    var cantResolve = ctor.GetParameters().Where(p => !container.CanResolve(new TypeKey(p.ParameterType, key)) && !p.HasDefaultValue).FirstOrDefault();
                    if (cantResolve != null)
                        throw new StyletIoCFindConstructorException(String.Format("Found a constructor with [Inject] on type {0}, but can't resolve parameter '{1}' (which doesn't have a default value).", this.Type.Name, cantResolve.Name));
                }
                else
                {
                    ctor = this.Type.GetConstructors()
                        .Where(c => c.GetParameters().All(p => container.CanResolve(new TypeKey(p.ParameterType, this.KeyForParameter(p))) || p.HasDefaultValue))
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
                    if (container.CanResolve(new TypeKey(x.ParameterType, key)))
                    {
                        try
                        {
                            return container.GetExpression(new TypeKey(x.ParameterType, key), true);
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

            public override Expression GetInstanceExpression(StyletIoC container)
            {
                var expr = (Expression<Func<T>>)(() => this.factory(container));
                return Expression.Invoke(expr, null);
            }
        }

        #endregion

        #region UnboundGeneric stuff

        private class UnboundGeneric
        {
            public bool WasAutoCreated { get; set; }
            public string Key { get; set; }
            public Type Type { get; private set; }
            public int NumTypeParams
            {
                get { return this.Type.GetTypeInfo().GenericTypeParameters.Length; }
            }
            public bool IsSingleton { get; private set; }

            public UnboundGeneric(Type type, bool isSingleton)
            {
                this.Type = type;
            }

            public IRegistration CreateRegistrationForType(Type boundType)
            {
                if (this.IsSingleton)
                    return new SingletonRegistration(new TypeCreator(boundType)) { WasAutoCreated = this.WasAutoCreated };
                else
                    return new TransientRegistration(new TypeCreator(boundType)) { WasAutoCreated = this.WasAutoCreated };
            }
        }

        #endregion

        private class TypeKey
        {
            public readonly Type Type;
            public readonly string Key;

            public TypeKey(Type type, string key)
            {
                this.Type = type;
                this.Key = key;
            }

            public override int GetHashCode()
            {
                if (this.Key == null)
                    return this.Type.GetHashCode();
                return this.Type.GetHashCode() ^ this.Key.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TypeKey))
                    return false;
                var other = (TypeKey)obj;
                return other.Type == this.Type && other.Key == this.Key;
            }
        }

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
