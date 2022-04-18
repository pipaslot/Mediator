using Pipaslot.Mediator.Http.Serialization.V3.Converters;
using System;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization
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
