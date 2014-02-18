using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StyletIoC
{
    public interface IContainer
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
        object GetTypeOrAll(Type type, string key = null);
        T GetTypeOrAll<T>(string key = null);
        void BuildUp(object item);
    }

    public class StyletIoCContainer : IContainer
    {
        public static readonly string FactoryAssemblyName = "StyletIoCFactory";

        private ConcurrentDictionary<TypeKey, IRegistrationCollection> registrations = new ConcurrentDictionary<TypeKey, IRegistrationCollection>();
        private ConcurrentDictionary<TypeKey, IRegistration> getAllRegistrations = new ConcurrentDictionary<TypeKey, IRegistration>();
        // The list object is used for locking it
        private ConcurrentDictionary<TypeKey, List<UnboundGeneric>> unboundGenerics = new ConcurrentDictionary<TypeKey, List<UnboundGeneric>>();
        private ConcurrentDictionary<Type, BuilderUpper> builderUppers = new ConcurrentDictionary<Type, BuilderUpper>();


        private ModuleBuilder factoryBuilder;
        private ConcurrentDictionary<Type, Type> factories = new ConcurrentDictionary<Type, Type>();

        private bool compilationStarted;

        public StyletIoCContainer()
        {
            this.BindSingleton<IContainer>().ToFactory(c => this);
        }

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
            return new StyletIoCBindTo(this, serviceType, false);
        }

        public IStyletIoCBindTo BindSingleton<TService>()
        {
            return this.BindSingleton(typeof(TService));
        }

        public IStyletIoCBindTo BindSingleton(Type serviceType)
        {
            this.CheckCompilationStarted();
            return new StyletIoCBindTo(this, serviceType, true);
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
                        registration.GetGenerator();
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
            var generator = this.GetRegistrations(new TypeKey(type, key), false).GetSingle().GetGenerator();
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
            if (!this.TryRetrieveGetAllRegistrationFromElementType(typeKey, null, out registration))
                throw new StyletIoCRegistrationException(String.Format("Could not find registration for type {0} and key '{1}'", typeKey.Type.Name));
            var generator = registration.GetGenerator();
            return (IEnumerable<object>)generator();
        }

        public IEnumerable<T> GetAll<T>(string key = null)
        {
            return this.GetAll(typeof(T), key).Cast<T>();
        }

        public object GetTypeOrAll(Type type, string key = null)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            var generator = this.GetRegistrations(new TypeKey(type, key), true).GetSingle().GetGenerator();
            return generator();
        }

        public T GetTypeOrAll<T>(string key = null)
        {
            return (T)this.GetTypeOrAll(typeof(T), key);
        }

        public void BuildUp(object item)
        {
            var builderUpper = this.GetBuilderUpper(item.GetType());
            builderUpper.GetImplementor()(item);
        }


        internal bool CanResolve(TypeKey typeKey)
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

        private bool TryRetrieveGetAllRegistrationFromElementType(TypeKey elementTypeKey, Type collectionTypeOrNull, out IRegistration registration)
        {
            // TryGet first, as making the generic type is expensive
            // If it isn't present, and can be made, GetOrAdd to try and add it, but return the now-existing registration if someone beat us to it
            if (this.getAllRegistrations.TryGetValue(elementTypeKey, out registration))
                return true;

            var listType = typeof(List<>).MakeGenericType(elementTypeKey.Type);
            if (collectionTypeOrNull != null && !collectionTypeOrNull.IsAssignableFrom(listType))
                return false;

            registration = this.getAllRegistrations.GetOrAdd(elementTypeKey, x => new GetAllRegistration(listType, this) { Key = elementTypeKey.Key });
            return true;
        }

        // Returns the type of element if it's valid
        private bool TryEnsureGetAllRegistrationCreated(TypeKey typeKey, out IRegistration registration)
        {
            registration = null;
            var elementType = this.GetElementTypeFromCollectionType(typeKey);
            if (elementType == null)
                return false;

            return this.TryRetrieveGetAllRegistrationFromElementType(new TypeKey(elementType, typeKey.Key), typeKey.Type, out registration);
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

        internal Expression GetExpression(TypeKey typeKey, bool searchGetAllTypes)
        {
            return this.GetRegistrations(typeKey, searchGetAllTypes).GetSingle().GetInstanceExpression();
        }

        internal IRegistrationCollection GetRegistrations(TypeKey typeKey, bool searchGetAllTypes)
        {
            IRegistrationCollection registrations;

            // Try to get registrations. If there are none, see if we can add some from unbound generics
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

        internal IRegistrationCollection AddRegistration(TypeKey typeKey, IRegistration registration)
        {
            return this.registrations.AddOrUpdate(typeKey, x => new SingleRegistration(registration), (x, c) => c.AddRegistration(registration));
        }

        internal void AddUnboundGeneric(TypeKey typeKey, UnboundGeneric unboundGeneric)
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

        internal Type GetFactoryForType(Type serviceType)
        {
            if (!serviceType.IsInterface)
                throw new StyletIoCCreateFactoryException(String.Format("Unable to create a factory implementing type {0}, as it isn't an interface", serviceType.Name));

            // Have we built it already?
            Type factoryType;
            if (this.factories.TryGetValue(serviceType, out factoryType))
                return factoryType;

            if (this.factoryBuilder == null)
            {
                var assemblyName = new AssemblyName(FactoryAssemblyName);
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("StyletIoCFactoryModule");
                Interlocked.CompareExchange(ref this.factoryBuilder, moduleBuilder, null);
            }

            // If the service is 'ISomethingFactory', call out new class 'SomethingFactory'
            var typeBuilder = this.factoryBuilder.DefineType(serviceType.Name.Substring(1), TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(serviceType);

            // Define a field which holds a reference to this ioc container
            var containerField = typeBuilder.DefineField("container", typeof(IContainer), FieldAttributes.Private);

            // Add a constructor which takes one argument - the container - and sets the field
            // public Name(IContainer container)
            // {
            //    this.container = container;
            // }
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(IContainer) });
            var ilGenerator = ctorBuilder.GetILGenerator();
            // Load 'this' and the IOC container onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            // Store the IOC container in this.container
            ilGenerator.Emit(OpCodes.Stfld, containerField);
            ilGenerator.Emit(OpCodes.Ret);

            // These are needed by all methods, so get them now
            // IContainer.GetTypeOrAll(Type, string)
            var containerGetMethod = typeof(IContainer).GetMethod("GetTypeOrAll", new Type[] { typeof(Type), typeof(string) });
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

                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, methodInfo.ReturnType, parameters.Select(x => x.ParameterType).ToArray());
                var methodIlGenerator = methodBuilder.GetILGenerator();
                // Load 'this' onto stack
                // Stack: [this]
                methodIlGenerator.Emit(OpCodes.Ldarg_0);
                // Load value of 'container' field of 'this' onto stack
                // Stack: [this.container]
                methodIlGenerator.Emit(OpCodes.Ldfld, containerField);
                // New local variable which represents type to load
                LocalBuilder lb = methodIlGenerator.DeclareLocal(methodInfo.ReturnType);
                // Load this onto the stack. This is a RuntimeTypeHandle
                // Stack: [this.container, runtimeTypeHandleOfReturnType]
                methodIlGenerator.Emit(OpCodes.Ldtoken, lb.LocalType);
                // Invoke Type.GetTypeFromHandle with this
                // This is equivalent to calling typeof(T)
                // Stack: [this.container, typeof(returnType)]
                methodIlGenerator.Emit(OpCodes.Call, typeFromHandleMethod);
                // Load the given key (if it's a parameter), or null if it isn't, onto the stack
                // Stack: [this.container, typeof(returnType), key]
                if (parameters.Length == 0)
                    methodIlGenerator.Emit(OpCodes.Ldnull); // Load null as the key
                else
                    methodIlGenerator.Emit(OpCodes.Ldarg_1); // Load the given string as the key
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
                throw new StyletIoCCreateFactoryException(String.Format("Unable to create factory type for interface {0}. Ensure that the interface is public, or add [assembly: InternalsVisibleTo(StyletIoC.FactoryAssemblyName)] to your AssemblyInfo.cs", serviceType.Name), e);
            }
            var actualType = this.factories.GetOrAdd(serviceType, constructedType);
            return actualType;
        }

        internal BuilderUpper GetBuilderUpper(Type type)
        {
            return this.builderUppers.GetOrAdd(type, x => new BuilderUpper(type, this));
        }
    }

    internal class TypeKey
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

    public interface IInjectionAware
    {
        void ParametersInjected();
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

    public class StyletIoCCreateFactoryException : StyletIoCException
    {
        public StyletIoCCreateFactoryException(string message) : base(message) { }
        public StyletIoCCreateFactoryException(string message, Exception innerException) : base(message, innerException) { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
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
