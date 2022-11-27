using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.V3.Converters
{
    internal class InterfaceConverterFactory : JsonConverterFactory
    {
        private readonly ICredibleProvider _credibleProvider;

        public InterfaceConverterFactory(ICredibleProvider credibleProvider)
        {
            _credibleProvider = credibleProvider;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.IsInterface)
            {
                return true;
            }
            return typeToConvert.IsInterface;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converter = (JsonConverter?)Activator.CreateInstance(
                typeof(InterfaceConverter<>).MakeGenericType(typeToConvert),
                new object[] { _credibleProvider }
                );

            return converter ?? throw MediatorHttpException.CreateForNotConstructableJsonConverter();
        }
    }
}
