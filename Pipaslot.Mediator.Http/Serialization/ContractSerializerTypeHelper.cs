using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization;

internal class ContractSerializerTypeHelper
{
    private const string _ignoredTypeSuffix = ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

    internal static string GetIdentifier(Type type)
    {
        var strType = type.AssemblyQualifiedName ?? string.Empty;
        if (strType.EndsWith(_ignoredTypeSuffix))
        {
            return strType.Substring(0, strType.Length - _ignoredTypeSuffix.Length);
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
                throw new Exception(
                    $"Can not recognize type {type} from received response. Ensure that type returned and serialized on server is available/referenced on client as well.");
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

        return RemoveAssemblySuffix(fullTypeAsString);
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
        if (value == null)
        {
            throw new NotSupportedException(
                $"Data type '{type}' is not supported for result serialization in the Mediator. Also avoid returning NULL.");
        }

        HashSet<string?>? parameterNames = null;

        var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip if no public getter
            if (property.GetMethod == null || !property.GetMethod.IsPublic)
            {
                continue;
            }

            // Check if the property has an init setter or lacks a public setter
            bool requiresConstructor = property.SetMethod == null || !property.SetMethod.IsPublic;

            // Lazy-load constructor parsing only when necessary
            if (requiresConstructor && parameterNames == null)
            {
                var constructor = type.GetConstructors()
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault(); // Prefer constructor with most parameters

                parameterNames = constructor?
                                     .GetParameters()
                                     .Select(p => p.Name)
                                     .ToHashSet(StringComparer.OrdinalIgnoreCase)
                                 ?? [];
            }

            // Skip if no matching parameter in constructor
            if (requiresConstructor && parameterNames is not null && !parameterNames.Contains(property.Name))
            {
                continue;
            }

            var propertyValue = property.GetValue(value);
            if (propertyValue != null || (options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull))
            {
                var propertyName = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
                writer.WritePropertyName(propertyName);
                JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
            }
        }
    }
}