using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.Converters
{
    internal class JsonInterfaceConverter<T> : JsonConverter<T> where T : class
    {
        private readonly ICredibleActionProvider _credibleActions;

        public JsonInterfaceConverter(ICredibleActionProvider credibleActions)
        {
            _credibleActions = credibleActions;
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Utf8JsonReader readerClone = reader;
            if (readerClone.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = readerClone.GetString();
            if (propertyName != "$type")
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            string typeValue = readerClone.GetString();
            var entityType = ContractSerializerTypeHelper.GetType(typeValue);
            _credibleActions.VerifyCredibility(entityType);

            var deserialized = JsonSerializer.Deserialize(ref reader, entityType, options);
            return (T)deserialized;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (T)null, options);
                    break;
                default:
                    {
                        var type = value.GetType();
                        //TODO Get rid of parse
                        using var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(value, type, options));
                        writer.WriteStartObject();
                        writer.WriteString("$type", ContractSerializerTypeHelper.GetIdentifier(type));

                        foreach (var element in jsonDocument.RootElement.EnumerateObject())
                        {
                            element.WriteTo(writer);
                        }

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}
