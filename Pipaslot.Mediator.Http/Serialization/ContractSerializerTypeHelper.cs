using System;
using System.Collections;
using System.Linq;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Serialization
{
    internal class ContractSerializerTypeHelper
    {
        private const string IgnoredTypeSuffix = ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        internal static string GetIdentifier(Type type)
        {
            var strType = type.AssemblyQualifiedName;
            if (strType.EndsWith(IgnoredTypeSuffix))
            {
                return strType.Substring(0, strType.Length - IgnoredTypeSuffix.Length);
            }
            return strType;
        }

        internal static Type? GetEnumeratedType(Type type)
        {
            if (type.IsArray)
            {
                var elType = type.GetElementType();
                if (null != elType)
                {
                    return elType;
                }
            }

            if (IsEnumerable(type))
            {
                var elTypes = type.GetGenericArguments();
                if (elTypes.Length > 0)
                {
                    return elTypes[0];
                }
            }
            return null;
        }

        internal static bool IsEnumerable(Type type)
        {
            return type.IsClass && type.GetInterfaces().Any(x => x == typeof(IEnumerable));
        }

        internal static Type GetType(string type)
        {
            var queryType = Type.GetType(type);
            if (queryType == null)
            {
                queryType = Type.GetType(GetTypeWithoutAssembly(type));
                if (queryType == null)
                {
                    throw new Exception($"Can not recognize type {type} from received response. Ensure that type returned and serialized on server is available/referenced on client as well.");
                }
            }
            return queryType;
        }

        /// <summary>
        /// This method converst type Definition like "System.Collections.Generic.List`1[[MyType, MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=none"
        /// to "System.Collections.Generic.List`1[[MyType, MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"
        /// This is fix providing backward compatibility for .NET Core 3.1 and older where was issue with parsing types belonging to assembly System.Private.CoreLib, because this assembly is not available or the version might be different
        /// </summary>
        /// <param name="fullTypeAsString"></param>
        /// <returns></returns>
        internal static string GetTypeWithoutAssembly(string fullTypeAsString)
        {
            var isGeneric = fullTypeAsString.Contains("]]");
            if (isGeneric)
            {
                var startIndex = fullTypeAsString.IndexOf("[[") + 2;
                var endIndex = fullTypeAsString.LastIndexOf("]]");
                var before = fullTypeAsString.Substring(0, startIndex);
                var between = fullTypeAsString.Substring(startIndex, endIndex - startIndex);
                var after = fullTypeAsString.Substring(endIndex);
                return before + GetTypeWithoutAssembly(between) + RemoveAssemblySuffix(after);
            }
            else
            {
                return RemoveAssemblySuffix(fullTypeAsString);
            }
        }

        private static string RemoveAssemblySuffix(string typeAsString)
        {
            var assemblyIndex = typeAsString.LastIndexOf(", System.Private.CoreLib");

            return assemblyIndex >= 0
                ? typeAsString.Substring(0, assemblyIndex)
                : typeAsString;
        }

        internal static void WriteObjectProperties<T>(Utf8JsonWriter writer, T value, Type type, JsonSerializerOptions options)
        {
            using var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(value, type, options));
            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var element in jsonDocument.RootElement.EnumerateObject())
                {
                    element.WriteTo(writer);
                }
            }
            else
            {
                throw new NotSupportedException($"Can not serialize type '{type}' as object because the class inherits from collection type.");
            }
        }
    }
}
