﻿using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.V3.Converters;

internal class InterfaceConverter<T>(ICredibleProvider credibleActions) : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var readerClone = reader;
        if (readerClone.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        readerClone.Read();
        if (readerClone.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        if (!readerClone.ValueTextEquals("$type"u8))
        {
            throw new JsonException();
        }

        readerClone.Read();
        if (readerClone.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        var typeValue = readerClone.GetString() ?? string.Empty;
        var resultType = ContractSerializerTypeHelper.GetType(typeValue);
        var arrayItemType = ContractSerializerTypeHelper.GetEnumeratedType(resultType);
        if (arrayItemType != null)
        {
            if (!arrayItemType.IsInterface)
            {
                // Ignored for arrays because interface array has type specfied for every member
                // and the type will be verified by interface converter
                credibleActions.VerifyCredibility(resultType);
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Property was expected");
            }

            if (!readerClone.ValueTextEquals("Items"u8))
            {
                throw new JsonException("Property with name Items was expected");
            }

            reader.Read();
            reader.Read();
            reader.Read();
        }
        else
        {
            credibleActions.VerifyCredibility(resultType);
        }

        var des = JsonSerializer.Deserialize(ref reader, resultType, options)
                  ?? throw new MediatorException($"Can not deserialize json to type {resultType}");
        return (T)des;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                JsonSerializer.Serialize(writer, "", options);
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