using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StyletIoC.Internal
{
    /// <summary>
    /// Useful extension methods on Type
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Return all base types and interfaces implemented by the given type (and its ancestors)
        /// </summary>
        /// <param name="type">Type to return base types and interfaces for</param>
        /// <returns>Base types and interfaces implemented by the given type</returns>
        public static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            return type.GetInterfaces().Concat(type.GetBaseTypes());
        }

        /// <summary>
        /// Return all base types implemented by the given type (and their base types, etc)
        /// </summary>
        /// <param name="type">Type to interrogate</param>
        /// <returns>Base types implemented by the given type</returns>
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                yield return baseType;
            }
        }

        /// <summary>
        /// Determine if any of the type's base types or interfaces is equal to the given service type. Also checks generic types
        /// </summary>
        /// <remarks>
        /// For example, given I1{T} and C1{T} : I1{T}, typeof(C1{int}).Implemements(typeof(I1{}) returns true.
        /// </remarks>
        /// <param name="implementationType">Implementation type</param>
        /// <param name="serviceType">Service type</param>
        /// <returns>Whether the implementation type implements the service type</returns>
        public static bool Implements(this Type implementationType, Type serviceType)
        {
            return serviceType.IsAssignableFrom(implementationType) ||
                (implementationType.IsGenericType && serviceType.IsGenericTypeDefinition && serviceType.IsAssignableFrom(implementationType.GetGenericTypeDefinition())) ||
                implementationType.GetBaseTypesAndInterfaces().Any(x => x == serviceType || (x.IsGenericType && x.GetGenericTypeDefinition() == serviceType));
        }

        private static readonly Dictionary<Type, string> primitiveNameMapping = new Dictionary<Type, string>()
        {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(bool), "bool" },
            { typeof(string), "string" },
        };

        /// <summary>
        /// Return a human-readable description of the given type
        /// </summary>
        /// <remarks>
        /// This returns things like 'List{int}' instead of 'List`1[System.Int32]'
        /// </remarks>
        /// <param name="type">Type to generate the description for</param>
        /// <returns>Description of the given type</returns>
        public static string GetDescription(this Type type)
        {
            if (type.IsGenericTypeDefinition)
                return String.Format("{0}<{1}>", type.Name.Split('`')[0], String.Join(", ", type.GetTypeInfo().GenericTypeParameters.Select(x => x.Name)));
            var genericArguments = type.GetGenericArguments();

            string name;
            if (genericArguments.Length > 0)
            {
                var genericArgumentNames = genericArguments.Select(x => primitiveNameMapping.TryGetValue(x, out name) ? name : x.Name);
                return String.Format("{0}<{1}>", type.Name.Split('`')[0], String.Join(", ", genericArgumentNames));
            }
            else
            {
                return primitiveNameMapping.TryGetValue(type, out name) ? name : type.Name;
            }
        }
    }
}
