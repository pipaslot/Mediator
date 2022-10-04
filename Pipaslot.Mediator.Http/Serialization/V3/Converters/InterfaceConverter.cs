﻿using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.V3.Converters
{
    internal class InterfaceConverter<T> : JsonConverter<T>
    {
        private readonly ICredibleProvider _credibleActions;

        public InterfaceConverter(ICredibleProvider credibleActions)
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
            var resultType = ContractSerializerTypeHelper.GetType(typeValue);
            var arrayItemType = ContractSerializerTypeHelper.GetEnumeratedType(resultType);
            if (arrayItemType != null)
            {
                if (!arrayItemType.IsInterface)
                {
                    // Ignored for arrays because interface array has type specfied for every member
                    // and the type will be verified by interface converter
                    _credibleActions.VerifyCredibility(resultType);
                }
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
                reader.Read();
                reader.Read();
                reader.Read();
                return (T)JsonSerializer.Deserialize(ref reader, resultType, options)
                    ?? throw new MediatorException($"Can not deserialize json to type {resultType}");
            }
            else
            {
                _credibleActions.VerifyCredibility(resultType);
                return (T)JsonSerializer.Deserialize(ref reader, resultType, options)
                    ?? throw new MediatorException($"Can not deserialize json to type {resultType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, null, options);
                    break;
                default:
                    {
                        var type = value.GetType();
                        writer.WriteStartObject();
                        writer.WriteString("$type", ContractSerializerTypeHelper.GetIdentifier(type));
                        if (ContractSerializerTypeHelper.IsEnumerable(type))
                        {
                            writer.WritePropertyName("Items");
                            JsonSerializer.Serialize(writer, value, type, options);
                        }
                        else
                        {
                            ContractSerializerTypeHelper.WriteObjectProperties(writer, value, type, options);
                        }
                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}
