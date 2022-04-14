using System;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Converters
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    internal class JsonInterfaceConverterAttribute : JsonConverterAttribute
    {
        public JsonInterfaceConverterAttribute(Type dataType) : base(GetConverterType(dataType))
        {
        }

        private static Type GetConverterType(Type dataType)
        {
            return typeof(JsonInterfaceConverter<>).MakeGenericType(dataType);
        }
    }
}
