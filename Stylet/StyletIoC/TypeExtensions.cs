using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace StyletIoC
{
    internal static class TypeExtensions
    {
        public static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            return type.GetInterfaces().Concat(type.GetBaseTypes());
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                yield return baseType;
            }
        }

        public static bool Implements(this Type implementationType, Type serviceType)
        {
            return serviceType.IsAssignableFrom(implementationType) ||
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
        };

        public static string Description(this Type type)
        {
            if (type.IsGenericTypeDefinition)
                return String.Format("{0}<{1}>", type.Name.Split('`')[0], String.Join(", ", type.GetTypeInfo().GenericTypeParameters.Select(x => x.Name)));
            var genericArguments = type.GetGenericArguments();
            if (genericArguments.Length > 0)
            {
                return String.Format("{0}<{1}>", type.Name.Split('`')[0], String.Join(", ", genericArguments.Select(x =>
                {
                    string name;
                    return primitiveNameMapping.TryGetValue(x, out name) ? name : x.Name;
                })));
            }
            return type.Name;
        }
    }
}
