using StyletIoC.Creation;
using StyletIoC.Internal.Creators;
using StyletIoC.Internal.RegistrationCollections;
using StyletIoC.Internal.Registrations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;


namespace StyletIoC.Internal
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Internal class, but some documentation added for readability. StyleCop ignores 'Internal only' setting if some documentation exists on member")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:ElementReturnValueMustBeDocumented", Justification = "Internal class, but some documentation added for readability. StyleCop ignores 'Internal only' setting if some documentation exists on member")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1618:GenericTypeParametersMustBeDocumented", Justification = "Internal class, but some documentation added for readability. StyleCop ignores 'Internal only' setting if some documentation exists on member")]
    // ReSharper disable once RedundantExtendsListEntry
    internal class Container : IContainer, IRegistrationContext
    {
        private readonly List<Assembly> autobindAssemblies;

        /// <summary>
        /// Maps a [type, key] pair to a collection of registrations for that keypair. You can retrieve an instance of the type from the registration
        /// </summary>
        private readonly ConcurrentDictionary<TypeKey, IRegistrationCollection> registrations = new ConcurrentDictionary<TypeKey, IRegistrationCollection>();

        /// <summary>
        /// Maps a [type, key] pair, where 'type' is the T in IEnumerable{T}, to a registration which can create a List{T} implementing that IEnumerable.
        /// This is separate from 'registrations' as some code paths - e.g. Get() - won't search it (while things like constructor/property injection will).
        /// </summary>
        private readonly ConcurrentDictionary<TypeKey, IRegistration> getAllRegistrations = new ConcurrentDictionary<TypeKey, IRegistration>();

        /// <summary>
        /// Maps a [type, key] pair, where 'type' is an unbound generic (something like IValidator{}) to something which, given a type, can create an IRegistration for that type.
        /// So if they've bound an IValidator{} to a an IntValidator, StringValidator, etc, and request an IValidator{string}, one of the UnboundGenerics here can generatr a StringValidator.
        /// </summary>
        /// <remarks>Dictionary{TKey, TValue} and List{T} are thread-safe for concurrent reads, which is all that happens after building</remarks>
        private readonly Dictionary<TypeKey, List<UnboundGeneric>> unboundGenerics = new Dictionary<TypeKey, List<UnboundGeneric>>();

        /// <summary>
        /// Maps a type onto a BuilderUpper for that type, which can create an Expresson/Delegate to build up that type.
        /// </summary>
        private readonly ConcurrentDictionary<RuntimeTypeHandle, BuilderUpper> builderUppers = new ConcurrentDictionary<RuntimeTypeHandle, BuilderUpper>();

        /// <summary>
        /// Builder used to build abstract factories
        /// </summary>
        private AbstractFactoryBuilder abstractFactoryBuilder;

        /// <summary>
        /// Fired when this container is asked to dispose
        /// </summary>
        public event EventHandler Disposing;

        private bool disposed;

        public Container(List<Assembly> autobindAssemblies)
        {
            this.autobindAssemblies = autobindAssemblies ?? new List<Assembly>();
        }

        /// <summary>
        /// Compile all known bindings (which would otherwise be compiled when needed), checking the dependency graph for consistency
        /// </summary>
        public void Compile(bool throwOnError = true)
        {
            foreach (var value in this.registrations.Values)
            {
                foreach (var registration in value.GetAll())
                {
                    try
                    {
                        registration.GetGenerator();
                    }
                    catch (StyletIoCFindConstructorException)
                    {
                        // If they've asked us to be quiet, we will
                        if (throwOnError)
                            throw;
                    }
                }
            }
        }

        /// <summary>
        /// Fetch a single instance of the specified type
        /// </summary>
        public object Get(Type type, string key = null)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return this.Get(type.TypeHandle, key, type);
        }

        /// <summary>
        /// Generic form of Get
        /// </summary>
        public T Get<T>(string key = null)
        {
            return (T)this.Get(typeof(T).TypeHandle, key);
        }

        private object Get(RuntimeTypeHandle typeHandle, string key = null, Type typeIfAvailable = null)
        {
            var generator = this.GetRegistrations(new TypeKey(typeHandle, key), false, typeIfAvailable).GetSingle().GetGenerator();
            return generator(this);
        }

        /// <summary>
        /// Fetch instances of all types which implement the specified service
        /// </summary>
        public IEnumerable<object> GetAll(Type type, string key = null)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return this.GetAll(type.TypeHandle, key, type);
        }

        /// <summary>
        /// Generic form of GetAll
        /// </summary>
        public IEnumerable<T> GetAll<T>(string key = null)
        {
            return this.GetAll(typeof(T).TypeHandle, key).Cast<T>();
        }

        private IEnumerable<object> GetAll(RuntimeTypeHandle typeHandle, string key = null, Type elementTypeIfAvailable = null)
        {
            var typeKey = new TypeKey(typeHandle, key);
            IRegistration registration;
            // This can currently never fail, since we pass in null
            var result = this.TryRetrieveGetAllRegistrationFromElementType(typeKey, null, out registration, elementTypeIfAvailable);
            Debug.Assert(result);
            var generator = registration.GetGenerator();
            return (IEnumerable<object>)generator(this);
        }

        /// <summary>
        /// If type is an IEnumerable{T} or similar, is equivalent to calling GetAll{T}. Else, is equivalent to calling Get{T}.
        /// </summary>
        public object GetTypeOrAll(Type type, string key = null)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return this.GetTypeOrAll(type.TypeHandle, key, type);
        }

        /// <summary>
        /// Generic form of GetTypeOrAll
        /// </summary>
        public T GetTypeOrAll<T>(string key = null)
        {
            return (T)this.GetTypeOrAll(typeof(T).TypeHandle, key);
        }

        private object GetTypeOrAll(RuntimeTypeHandle typeHandle, string key = null, Type typeIfAvailable = null)
        {
            var generator = this.GetRegistrations(new TypeKey(typeHandle, key), true, typeIfAvailable).GetSingle().GetGenerator();
            return generator(this);
        }

        /// <summary>
        /// For each property/field with the [Inject] attribute, sets it to an instance of that type
        /// </summary>
        public void BuildUp(object item)
        {
            var builderUpper = this.GetBuilderUpper(item.GetType());
            builderUpper.GetImplementor()(this, item);
        }

        /// <summary>
        /// Determine whether we can resolve a particular typeKey
        /// </summary>
        bool IRegistrationContext.CanResolve(Type type, string key)
        {
            IRegistrationCollection registrations;

            if (this.registrations.TryGetValue(new TypeKey(type.TypeHandle, key), out registrations) ||
                this.TryCreateFuncFactory(type, key, out registrations) ||
                this.TryCreateGenericTypesForUnboundGeneric(type, key, out registrations) ||
                this.TryCreateSelfBinding(type, key, out registrations))
            {
                return true;
            }

            // Is it a 'get all' request?
            IRegistration registration;
            return this.TryRetrieveGetAllRegistration(type, key, out registration);
        }

        /// <summary>
        /// Given a collection type (IEnumerable{T}, etc) extracts the T, or null if we couldn't, or if we can't resolve that [T, key]
        /// </summary>
        private Type GetElementTypeFromCollectionType(Type type)
        {
            // Elements are never removed from this.registrations, so we're safe to make this ContainsKey query
            if (!type.IsGenericType || !type.Implements(typeof(IEnumerable<>)))
                return null;
            return type.GenericTypeArguments[0];
        }

        /// <summary>
        /// Given an type, tries to create or fetch an IRegistration which can create an IEnumerable{T}. If collectionTypeOrNull is given, ensures that the generated
        /// implementation of the IEnumerable{T} is compatible with that collection (e.g. if they've requested a List{T} in a constructor param, collectionTypeOrNull will be List{T}).
        /// </summary>
        /// <param name="elementTypeKey">Element type and key to create an IRegistration for</param>
        /// <param name="collectionTypeOrNull">If given (not null), ensures that the generated implementation of the collection is compatible with this</param>
        /// <param name="registration">Returned IRegistration, or null if the method returns false</param>
        /// <param name="elementTypeIfAvailable">Type corresponding to elementTypeKey, if available. Used for a small optimization</param>
        /// <returns>Whether such an IRegistration could be created or retrieved</returns>
        private bool TryRetrieveGetAllRegistrationFromElementType(TypeKey elementTypeKey, Type collectionTypeOrNull, out IRegistration registration, Type elementTypeIfAvailable = null)
        {
            // TryGet first, as making the generic type is expensive
            // If it isn't present, and can be made, GetOrAdd to try and add it, but return the now-existing registration if someone beat us to it
            if (this.getAllRegistrations.TryGetValue(elementTypeKey, out registration))
                return true;

            // Failed :( Have to fetch the Type
            var elementType = elementTypeIfAvailable ?? Type.GetTypeFromHandle(elementTypeKey.TypeHandle);
            var listType = typeof(List<>).MakeGenericType(elementType);
            if (collectionTypeOrNull != null && !collectionTypeOrNull.IsAssignableFrom(listType))
                return false;

            registration = this.getAllRegistrations.GetOrAdd(elementTypeKey, x => new GetAllRegistration(listType.TypeHandle, this, elementTypeKey.Key));
            return true;
        }

        /// <summary>
        /// Wrapper around TryRetrieveGetAllRegistrationFromElementType, which also extracts the element type from the collection type
        /// </summary>
        /// <param name="type">Type of the collection</param>
        /// <param name="key">Key associated with the collection</param>
        /// <param name="registration">Returned IRegistration, or null if the method returns false</param>
        /// <returns>Whether such an IRegistration could be created or retrieved</returns>
        private bool TryRetrieveGetAllRegistration(Type type, string key, out IRegistration registration)
        {
            registration = null;
            var elementType = this.GetElementTypeFromCollectionType(type);
            if (elementType == null)
                return false;

            return this.TryRetrieveGetAllRegistrationFromElementType(new TypeKey(elementType.TypeHandle, key), type, out registration, elementType);
        }

        private bool TryCreateSelfBinding(Type type, string key, out IRegistrationCollection registrations)
        {
            registrations = null;

            if (type.IsAbstract || !type.IsClass)
                return false;

            var injectAttribute = type.GetCustomAttribute<InjectAttribute>(true);
            if (injectAttribute != null && injectAttribute.Key != key)
                return false;

            // Only allow types in our whitelisted assemblies
            // This stops us trying to charge off and create List<T> or some other BCL class which we don't have a hope in hell of creating
            // This in turn leads to some very hard-to-debug error cases where we descend into infinite recursion on some random type
            if (!this.autobindAssemblies.Contains(type.Assembly))
                return false;

            var typeKey = new TypeKey(type.TypeHandle, key);
            registrations = this.AddRegistration(typeKey, new TransientRegistration(new TypeCreator(type, this)));
            return true;
        }

        /// <summary>
        /// If the given type is a Func{T}, get a registration which can create an instance of it
        /// </summary>
        private bool TryCreateFuncFactory(Type type, string key, out IRegistrationCollection registrations)
        {
            registrations = null;

            if (!type.IsGenericType)
                return false;

            var genericType = type.GetGenericTypeDefinition();
            var genericArguments = type.GetGenericArguments();

            if (genericType == typeof(Func<>))
            {
                var typeKey = new TypeKey(type.TypeHandle, key);
                foreach (var registration in this.GetRegistrations(new TypeKey(genericArguments[0].TypeHandle, key), true, genericArguments[0]).GetAll())
                {
                    registrations = this.AddRegistration(typeKey, new FuncRegistration(registration));
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Given a generic type (e.g. IValidator{T}), tries to create a collection of IRegistrations which can implement it from the unbound generic registrations.
        /// For example, if someone bound an IValidator{} to Validator{}, and this was called with Validator{T}, the IRegistrationCollection would contain a Validator{T}.
        /// </summary>
        private bool TryCreateGenericTypesForUnboundGeneric(Type type, string key, out IRegistrationCollection registrations)
        {
            registrations = null;

            if (!type.IsGenericType || type.GenericTypeArguments.Length == 0)
                return false;

            Type unboundGenericType = type.GetGenericTypeDefinition();

            List<UnboundGeneric> unboundGenerics;
            if (!this.unboundGenerics.TryGetValue(new TypeKey(unboundGenericType.TypeHandle, key), out unboundGenerics))
                return false;

            foreach (var unboundGeneric in unboundGenerics)
            {
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

                // The binder should have made sure of this
                Debug.Assert(type.IsAssignableFrom(newType));

                // Right! We've made a new generic type we can use
                var registration = unboundGeneric.CreateRegistrationForTypeAndKey(newType, key);

                // AddRegistration returns the IRegistrationCollection which was added/updated, so the one returned from the final
                // call to AddRegistration is the final IRegistrationCollection for this key
                registrations = this.AddRegistration(new TypeKey(type.TypeHandle, key), registration);
            }

            return registrations != null;
        }

        IRegistration IRegistrationContext.GetSingleRegistration(Type type, string key, bool searchGetAllTypes)
        {
            return this.GetRegistrations(new TypeKey(type.TypeHandle, key), searchGetAllTypes, type).GetSingle();
        }

        IReadOnlyList<IRegistration> IRegistrationContext.GetAllRegistrations(Type type, string key, bool searchGetAllTypes)
        {
            return this.GetRegistrations(new TypeKey(type.TypeHandle, key), searchGetAllTypes, type).GetAll();
        }

        internal IReadOnlyRegistrationCollection GetRegistrations(TypeKey typeKey, bool searchGetAllTypes, Type typeIfAvailable = null)
        {
            this.CheckDisposed();

            IReadOnlyRegistrationCollection readOnlyRegistrations;

            IRegistrationCollection registrations;
            if (this.registrations.TryGetValue(typeKey, out registrations))
            {
                readOnlyRegistrations = registrations;
            }
            else
            {
                // At this point we need to fetch the type from its handle
                // This is the rare path - once we've hit it once, the result is cached in registrations
                var type = typeIfAvailable ?? Type.GetTypeFromHandle(typeKey.TypeHandle);
                if (this.TryCreateFuncFactory(type, typeKey.Key, out registrations) ||
                    this.TryCreateGenericTypesForUnboundGeneric(type, typeKey.Key, out registrations) ||
                    this.TryCreateSelfBinding(type, typeKey.Key, out registrations))
                {
                    readOnlyRegistrations = registrations;
                }
                else if (searchGetAllTypes)
                {
                    // Couldn't find this type - is it a 'get all' collection type? (i.e. they've put IEnumerable<TypeWeCanResolve> in a ctor param)
                    IRegistration registration;
                    if (!this.TryRetrieveGetAllRegistration(type, typeKey.Key, out registration))
                        throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0}.", type.GetDescription()));

                    // Got this far? Good. There's actually a 'get all' collection type. Proceed with that
                    readOnlyRegistrations = new SingleRegistration(registration);
                }
                else
                {
                    // This will throw a StyletIoCRegistrationException if GetSingle is requested
                    readOnlyRegistrations = new EmptyRegistrationCollection(type);
                }
            }

            return readOnlyRegistrations;
        }

        internal IRegistrationCollection AddRegistration(TypeKey typeKey, IRegistration registration)
        {
            try
            {
                return this.registrations.AddOrUpdate(typeKey, x => new SingleRegistration(registration), (x, c) => c.AddRegistration(registration));
            }
            catch (StyletIoCRegistrationException e)
            {
                throw new StyletIoCRegistrationException(String.Format("{0} Service type: {1}, key: '{2}'", e.Message, Type.GetTypeFromHandle(typeKey.TypeHandle).GetDescription(), typeKey.Key), e);
            }
        }

        internal void AddUnboundGeneric(TypeKey typeKey, UnboundGeneric unboundGeneric)
        {
            // We're not worried about thread-safety across multiple calls to this function (as it's only called as part of setup, which we're
            // not thread-safe about).
            List<UnboundGeneric> unboundGenerics;
            if (!this.unboundGenerics.TryGetValue(typeKey, out unboundGenerics))
            {
                unboundGenerics = new List<UnboundGeneric>();
                this.unboundGenerics.Add(typeKey, unboundGenerics);
            }
            // Is there an existing registration for this type?
            if (unboundGenerics.Any(x => x.Type == unboundGeneric.Type))
                throw new StyletIoCRegistrationException(String.Format("Multiple registrations for type {0} found", Type.GetTypeFromHandle(typeKey.TypeHandle).GetDescription()));

            unboundGenerics.Add(unboundGeneric);
        }

        internal Type GetFactoryForType(Type serviceType)
        {
            if (this.abstractFactoryBuilder == null)
                this.abstractFactoryBuilder = new AbstractFactoryBuilder();

            return this.abstractFactoryBuilder.GetFactoryForType(serviceType);
        }

        public BuilderUpper GetBuilderUpper(Type type)
        {
            var typeHandle = type.TypeHandle;
            return this.builderUppers.GetOrAdd(typeHandle, x => new BuilderUpper(typeHandle, this));
        }

        public void Dispose()
        {
            this.disposed = true;
            var handler = this.Disposing;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void CheckDisposed()
        {
            if (this.disposed)
                throw new ObjectDisposedException("IContainer");
        }
    }
}
