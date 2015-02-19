using StyletIoC.Creation;
using StyletIoC.Internal.RegistrationCollections;
using StyletIoC.Internal.Registrations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StyletIoC.Internal
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Internal class, but some documentation added for readability. StyleCop ignores 'Internal only' setting if some documentation exists on member")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:ElementReturnValueMustBeDocumented", Justification = "Internal class, but some documentation added for readability. StyleCop ignores 'Internal only' setting if some documentation exists on member")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1618:GenericTypeParametersMustBeDocumented", Justification = "Internal class, but some documentation added for readability. StyleCop ignores 'Internal only' setting if some documentation exists on member")]
    // ReSharper disable once RedundantExtendsListEntry
    internal class Container : IContainer, IRegistrationContext
    {
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
        private readonly ConcurrentDictionary<Type, BuilderUpper> builderUppers = new ConcurrentDictionary<Type, BuilderUpper>();

        /// <summary>
        /// Cached ModuleBuilder used for building factory implementations
        /// </summary>
        private ModuleBuilder factoryBuilder;

        /// <summary>
        /// Fired when this container is asked to dispose
        /// </summary>
        public event EventHandler Disposing;

        private bool disposed;

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
            var generator = this.GetRegistrations(new TypeKey(type, key), false).GetSingle().GetGenerator();
            return generator(this);
        }

        /// <summary>
        /// Generic form of Get
        /// </summary>
        public T Get<T>(string key = null)
        {
            return (T)this.Get(typeof(T), key);
        }

        /// <summary>
        /// Fetch instances of all types which implement the specified service
        /// </summary>
        public IEnumerable<object> GetAll(Type type, string key = null)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            var typeKey = new TypeKey(type, key);
            IRegistration registration;
            // This can currently never fail, since we pass in null
            var result = this.TryRetrieveGetAllRegistrationFromElementType(typeKey, null, out registration);
            Debug.Assert(result);
            var generator = registration.GetGenerator();
            return (IEnumerable<object>)generator(this);
        }

        /// <summary>
        /// Generic form of GetAll
        /// </summary>
        public IEnumerable<T> GetAll<T>(string key = null)
        {
            return this.GetAll(typeof(T), key).Cast<T>();
        }

        /// <summary>
        /// If type is an IEnumerable{T} or similar, is equivalent to calling GetAll{T}. Else, is equivalent to calling Get{T}.
        /// </summary>
        public object GetTypeOrAll(Type type, string key = null)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            var generator = this.GetRegistrations(new TypeKey(type, key), true).GetSingle().GetGenerator();
            return generator(this);
        }

        /// <summary>
        /// Generic form of GetTypeOrAll
        /// </summary>
        public T GetTypeOrAll<T>(string key = null)
        {
            return (T)this.GetTypeOrAll(typeof(T), key);
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
            return this.CanResolve(new TypeKey(type, key));
        }

        internal bool CanResolve(TypeKey typeKey)
        {
            IRegistrationCollection registrations;

            if (this.registrations.TryGetValue(typeKey, out registrations) ||
                this.TryCreateFuncFactory(typeKey, out registrations) ||
                this.TryCreateGenericTypesForUnboundGeneric(typeKey, out registrations))
            {
                return true;
            }

            // Is it a 'get all' request?
            IRegistration registration;
            return this.TryRetrieveGetAllRegistration(typeKey, out registration);
        }

        /// <summary>
        /// Given a collection type (IEnumerable{T}, etc) extracts the T, or null if we couldn't, or if we can't resolve that [T, key]
        /// </summary>
        private Type GetElementTypeFromCollectionType(TypeKey typeKey)
        {
            Type type = typeKey.Type;
            // Elements are never removed from this.registrations, so we're safe to make this ContainsKey query
            if (!type.IsGenericType || !typeKey.Type.Implements(typeof(IEnumerable<>)))
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
        /// <returns>Whether such an IRegistration could be created or retrieved</returns>
        private bool TryRetrieveGetAllRegistrationFromElementType(TypeKey elementTypeKey, Type collectionTypeOrNull, out IRegistration registration)
        {
            // TryGet first, as making the generic type is expensive
            // If it isn't present, and can be made, GetOrAdd to try and add it, but return the now-existing registration if someone beat us to it
            if (this.getAllRegistrations.TryGetValue(elementTypeKey, out registration))
                return true;

            var listType = typeof(List<>).MakeGenericType(elementTypeKey.Type);
            if (collectionTypeOrNull != null && !collectionTypeOrNull.IsAssignableFrom(listType))
                return false;

            registration = this.getAllRegistrations.GetOrAdd(elementTypeKey, x => new GetAllRegistration(listType, this, elementTypeKey.Key));
            return true;
        }

        /// <summary>
        /// Wrapper around TryRetrieveGetAllRegistrationFromElementType, which also extracts the element type from the collection type
        /// </summary>
        /// <param name="typeKey">Type of the collection, and key associated with it</param>
        /// <param name="registration">Returned IRegistration, or null if the method returns false</param>
        /// <returns>Whether such an IRegistration could be created or retrieved</returns>
        private bool TryRetrieveGetAllRegistration(TypeKey typeKey, out IRegistration registration)
        {
            registration = null;
            var elementType = this.GetElementTypeFromCollectionType(typeKey);
            if (elementType == null)
                return false;

            return this.TryRetrieveGetAllRegistrationFromElementType(new TypeKey(elementType, typeKey.Key), typeKey.Type, out registration);
        }

        /// <summary>
        /// If the given type is a Func{T} or a Func{string, T}, get a registration which can create an instance of it
        /// </summary>
        private bool TryCreateFuncFactory(TypeKey typeKey, out IRegistrationCollection registrations)
        {
            registrations = null;
            var type = typeKey.Type;

            if (!type.IsGenericType)
                return false;

            var genericType = type.GetGenericTypeDefinition();
            var genericArguments = type.GetGenericArguments();

            if (genericType == typeof(Func<>))
            {
                foreach (var registration in this.GetRegistrations(new TypeKey(genericArguments[0], typeKey.Key), true).GetAll())
                {
                    registrations = this.AddRegistration(typeKey, new FuncNoKeyRegistration(registration));
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
                var registration = unboundGeneric.CreateRegistrationForTypeKey(new TypeKey(newType, typeKey.Key));

                // AddRegistration returns the IRegistrationCollection which was added/updated, so the one returned from the final
                // call to AddRegistration is the final IRegistrationCollection for this key
                registrations = this.AddRegistration(typeKey, registration);
            }

            return registrations != null;
        }

        IRegistration IRegistrationContext.GetSingleRegistration(Type type, string key, bool searchGetAllTypes)
        {
            return this.GetRegistrations(new TypeKey(type, key), searchGetAllTypes).GetSingle();
        }

        IReadOnlyList<IRegistration> IRegistrationContext.GetAllRegistrations(Type type, string key, bool searchGetAllTypes)
        {
            return this.GetRegistrations(new TypeKey(type, key), searchGetAllTypes).GetAll();
        }

        internal IRegistrationCollection GetRegistrations(TypeKey typeKey, bool searchGetAllTypes)
        {
            this.CheckDisposed();

            IRegistrationCollection registrations;

            // Try to get registrations. If there are none, see if we can add some from unbound generics
            if (!this.registrations.TryGetValue(typeKey, out registrations) &&
                !this.TryCreateFuncFactory(typeKey, out registrations) && 
                !this.TryCreateGenericTypesForUnboundGeneric(typeKey, out registrations))
            {
                if (searchGetAllTypes)
                {
                    // Couldn't find this type - is it a 'get all' collection type? (i.e. they've put IEnumerable<TypeWeCanResolve> in a ctor param)
                    IRegistration registration;
                    if (!this.TryRetrieveGetAllRegistration(typeKey, out registration))
                        throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0}.", typeKey.Type.GetDescription()));

                    // Got this far? Good. There's actually a 'get all' collection type. Proceed with that
                    registrations = new SingleRegistration(registration);
                }
                else
                {
                    // This will throw a StyletIoCRegistrationException if GetSingle is requested
                    registrations = new EmptyRegistrationCollection(typeKey.Type);
                }
            }

            return registrations;
        }

        internal IRegistrationCollection AddRegistration(TypeKey typeKey, IRegistration registration)
        {
            try
            {
                return this.registrations.AddOrUpdate(typeKey, x => new SingleRegistration(registration), (x, c) => c.AddRegistration(registration));
            }
            catch (StyletIoCRegistrationException e)
            {
                throw new StyletIoCRegistrationException(String.Format("{0} Service type: {1}, key: '{2}'", e.Message, typeKey.Type.GetDescription(), typeKey.Key), e);
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
                throw new StyletIoCRegistrationException(String.Format("Multiple registrations for type {0} found", typeKey.Type.GetDescription()));

            unboundGenerics.Add(unboundGeneric);
        }

        internal Type GetFactoryForType(Type serviceType)
        {
            // Not thread-safe, as it's only called from the builder
            if (!serviceType.IsInterface)
                throw new StyletIoCCreateFactoryException(String.Format("Unable to create a factory implementing type {0}, as it isn't an interface", serviceType.GetDescription()));

            if (this.factoryBuilder == null)
            {
                var assemblyName = new AssemblyName(StyletIoCContainer.FactoryAssemblyName);
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("StyletIoCFactoryModule");
                this.factoryBuilder = moduleBuilder;
            }

            // If the service is 'ISomethingFactory', call out new class 'SomethingFactory'
            var typeBuilder = this.factoryBuilder.DefineType(serviceType.Name.Substring(1), TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(serviceType);

            // Define a field which holds a reference to the registration context
            var registrationContextField = typeBuilder.DefineField("registrationContext", typeof(IRegistrationContext), FieldAttributes.Private);

            // Add a constructor which takes one argument - the container - and sets the field
            // public Name(IRegistrationContext registrationContext)
            // {
            //    this.registrationContext = registrationContext;
            // }
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IRegistrationContext) });
            var ilGenerator = ctorBuilder.GetILGenerator();
            // Load 'this' and the registration context onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            // Store the registration context in this.registrationContext
            ilGenerator.Emit(OpCodes.Stfld, registrationContextField);
            ilGenerator.Emit(OpCodes.Ret);

            // These are needed by all methods, so get them now
            // IRegistrationContext.GetTypeOrAll(Type, string)
            // IRegistrationContext extends ICreator, and it's ICreator that actually implements this
            var containerGetMethod = typeof(IContainer).GetMethod("GetTypeOrAll", new[] { typeof(Type), typeof(string) });
            // Type.GetTypeFromHandler(RuntimeTypeHandle)
            var typeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle");

            // Go through each method, emmitting an implementation for each
            foreach (var methodInfo in serviceType.GetMethods())
            {
                var parameters = methodInfo.GetParameters();
                if (!(parameters.Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))))
                    throw new StyletIoCCreateFactoryException("Can only implement methods with zero arguments, or a single string argument");

                if (methodInfo.ReturnType == typeof(void))
                    throw new StyletIoCCreateFactoryException("Can only implement methods which return something");

                var attribute = methodInfo.GetCustomAttribute<InjectAttribute>(true);

                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, methodInfo.ReturnType, parameters.Select(x => x.ParameterType).ToArray());
                var methodIlGenerator = methodBuilder.GetILGenerator();
                // Load 'this' onto stack
                // Stack: [this]
                methodIlGenerator.Emit(OpCodes.Ldarg_0);
                // Load value of 'registrationContext' field of 'this' onto stack
                // Stack: [this.registrationContext]
                methodIlGenerator.Emit(OpCodes.Ldfld, registrationContextField);
                // New local variable which represents type to load
                LocalBuilder lb = methodIlGenerator.DeclareLocal(methodInfo.ReturnType);
                // Load this onto the stack. This is a RuntimeTypeHandle
                // Stack: [this.registrationContext, runtimeTypeHandleOfReturnType]
                methodIlGenerator.Emit(OpCodes.Ldtoken, lb.LocalType);
                // Invoke Type.GetTypeFromHandle with this
                // This is equivalent to calling typeof(T)
                // Stack: [this.registrationContext, typeof(returnType)]
                methodIlGenerator.Emit(OpCodes.Call, typeFromHandleMethod);
                // Load the given key (if it's a parameter), or the key from the attribute if given, or null, onto the stack
                // Stack: [this.registrationContext, typeof(returnType), key]
                if (parameters.Length == 0)
                {
                    if (attribute == null)
                        methodIlGenerator.Emit(OpCodes.Ldnull);
                    else
                        methodIlGenerator.Emit(OpCodes.Ldstr, attribute.Key); // Load null as the key
                }
                else
                {
                    methodIlGenerator.Emit(OpCodes.Ldarg_1); // Load the given string as the key
                }
                // Call container.Get(type, key)
                // Stack: [returnedInstance]
                methodIlGenerator.Emit(OpCodes.Callvirt, containerGetMethod);
                methodIlGenerator.Emit(OpCodes.Ret);

                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }

            Type constructedType;
            try
            {
                constructedType = typeBuilder.CreateType();
            }
            catch (TypeLoadException e)
            {
                throw new StyletIoCCreateFactoryException(String.Format("Unable to create factory type for interface {0}. Ensure that the interface is public, or add [assembly: InternalsVisibleTo(StyletIoCContainer.FactoryAssemblyName)] to your AssemblyInfo.cs", serviceType.GetDescription()), e);
            }

            return constructedType;
        }

        public BuilderUpper GetBuilderUpper(Type type)
        {
            return this.builderUppers.GetOrAdd(type, x => new BuilderUpper(type, this));
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
