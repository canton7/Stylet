using StyletIoC;
using StyletIoC.Creation;
using StyletIoC.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace StyletIoC.Internal
{
    internal class AbstractFactoryBuilder
    {
        private readonly ModuleBuilder moduleBuilder;

        public AbstractFactoryBuilder()
        {
            var assemblyName = new AssemblyName(StyletIoCContainer.FactoryAssemblyName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule("StyletIoCFactoryModule");
            this.moduleBuilder = moduleBuilder;
        }

        public Type GetFactoryForType(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            // Not thread-safe, as it's only called from the builder
            if (!serviceType.IsInterface)
                throw new StyletIoCCreateFactoryException(String.Format("Unable to create a factory implementing type {0}, as it isn't an interface", serviceType.GetDescription()));

            // If the service is 'ISomethingFactory', call our new class 'GeneratedSomethingFactory'
            var typeBuilder = this.moduleBuilder.DefineType(this.CreateImplementationName(serviceType), TypeAttributes.Public);
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

        private void AddFriendlierNameForType(StringBuilder sb, Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType)
            {
                sb.Append(type.GetGenericTypeDefinition().FullName.Replace('.', '+'));
                sb.Append("<>["); // Just so that they can't fool us with carefully-crafted interface names...
                foreach (var arg in typeInfo.GetGenericArguments())
                {
                    this.AddFriendlierNameForType(sb, arg);
                }
                sb.Append("]");
            }
            else
            {
                sb.Append(type.FullName.Replace('.', '+'));
            }
            sb.Append("<>").Append(typeInfo.Assembly.GetName().Name.Replace('.', '+'));
        }

        private string CreateImplementationName(Type interfaceType)
        {
            var sb = new StringBuilder();
            // Make this unspeakable, just in case...
            sb.Append("Stylet.AutoGenerated.<>");
            this.AddFriendlierNameForType(sb, interfaceType);
            return sb.ToString();
        }

    }
}
