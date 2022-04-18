using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Linq;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Serialization.V3.Converters
{
    internal static class MediatorActionConvert
    {
        internal static object Read(
            ref Utf8JsonReader reader,
            JsonSerializerOptions options,
            ICredibleProvider credibleActions,
            out string typeValue)
        {
            Utf8JsonReader readerClone = reader;
            if (readerClone.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("StartObject was expected");
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Property was expected");
            }

            var propertyName = readerClone.GetString();
            if (propertyName != "$type")
            {
                throw new JsonException("Property with name $type was expected");
            }
            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Value was expected");
            }
            typeValue = readerClone.GetString() ?? "";
            if (typeValue == null)
            {
                throw new JsonException("Type value can not be null");
            }
            var actionType = ContractSerializerTypeHelper.GetType(typeValue);
            if (actionType.IsArray)
            {
                readerClone.Read();
                if (readerClone.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Property was expected");
                }

                propertyName = readerClone.GetString();
                if (propertyName != "Items")
                {
                    throw new JsonException("Property with name Items was expected");
                }
                readerClone.Read();
                return CreateObject(ref readerClone, options, credibleActions, typeValue, actionType);
            }
            return CreateObject(ref reader, options, credibleActions, typeValue, actionType);
        }

        private static object CreateObject(ref Utf8JsonReader reader, JsonSerializerOptions options, ICredibleProvider credibleActions, string typeValue, Type actionType)
        {
            credibleActions.VerifyCredibility(actionType);
            var result = JsonSerializer.Deserialize(ref reader, actionType, options);
            if (result == null)
            {
                throw new Exception($"Can not deserialize contract as type {typeValue} received from server");
            }
            return result;
        }

        internal static void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (object)null, options);
                    break;
                default:
                    {
                        var type = value.GetType();
                        //TODO Optimize
                        using var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(value, type, options));

                        writer.WriteStartObject();
                        writer.WriteString("$type", ContractSerializerTypeHelper.GetIdentifier(type));
                        if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            writer.WritePropertyName("Items");
                            writer.WriteStartArray();
                            foreach (var element in jsonDocument.RootElement.EnumerateArray())
                            {
                                element.WriteTo(writer);
                            }
                            writer.WriteEndArray();
                        }
                        else
                        {
                            foreach (var element in jsonDocument.RootElement.EnumerateObject())
                            {
                                element.WriteTo(writer);
                            }
                        }
                        writer.WriteEndObject();

                        break;
                    }
            }
        }
    }
}
