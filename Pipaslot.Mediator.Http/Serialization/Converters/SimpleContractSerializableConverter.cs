using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.Converters
{
    //TODO convert to IMediatorActionConverter
    internal class SimpleContractSerializableConverter : JsonConverter<ContractSerializable>
    {
        private readonly ICredibleActionProvider _credibleActions;

        public SimpleContractSerializableConverter(ICredibleActionProvider credibleActions)
        {
            _credibleActions = credibleActions;
        }

        public override ContractSerializable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
            var typeValue = readerClone.GetString();
            if (typeValue == null)
            {
                throw new JsonException("Type value can not be null");
            }
            return CreateObject(ref reader, typeValue, options);
        }

        private ContractSerializable CreateObject(ref Utf8JsonReader reader, string type, JsonSerializerOptions options)
        {
            var actionType = ContractSerializerTypeHelper.GetType(type);
            _credibleActions.VerifyCredibility(actionType);

            var result = JsonSerializer.Deserialize(ref reader, actionType, options);
            if (result == null)
            {
                throw new Exception($"Can not deserialize contract as type {type} received from server");
            }
            return new ContractSerializable(result, type);
        }

        public override void Write(Utf8JsonWriter writer, ContractSerializable value, JsonSerializerOptions options)
        {
            switch (value.Content)
            {
                case null:
                    JsonSerializer.Serialize(writer, (ContractSerializable)null, FullJsonContractSerializer.SerializationOptionsWithoutConverters);
                    break;
                default:
                    {
                        var type = value.Content.GetType();
                        using var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(value.Content, type, FullJsonContractSerializer.SerializationOptionsWithoutConverters));
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
