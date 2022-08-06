using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.V3.Converters
{
    internal class MediatorResponseConverter : JsonConverter<IMediatorResponse>
    {
        private readonly ICredibleProvider _credibleResults;

        public MediatorResponseConverter(ICredibleProvider credibleResults)
        {
            _credibleResults = credibleResults;
        }

        public override IMediatorResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var success = false;
            var errorMessages = new string[0];
            var results = new object[0];
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString() ?? "";
                    reader.Read();
                    switch (propertyName)
                    {
                        case nameof(IMediatorResponse.Success):
                            success = reader.GetBoolean();
                            break;
                        case nameof(IMediatorResponse.Results):
                            results = ReadResults(ref reader, options);
                            break;
                    }
                }
            }
            return new MediatorResponse(success, results);
        }
        private object[] ReadResults(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var results = new List<object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    continue;
                }
                results.Add(ReadResult(ref reader, options));
            }
            return results.ToArray();
        }

        private object ReadResult(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var readerClone = reader;
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
            var resultType = ContractSerializerTypeHelper.GetType(typeValue);
            var arrayItemType = ContractSerializerTypeHelper.GetEnumeratedType(resultType);
            if (arrayItemType != null)
            {
                if (!arrayItemType.IsInterface)
                {
                    // Ignored for arrays because interface array has type specfied for every member
                    // and the type will be verified by interface converter
                    _credibleResults.VerifyCredibility(resultType);
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
                return JsonSerializer.Deserialize(ref reader, resultType, options)
                    ?? throw new MediatorException($"Can not deserialize json to type {resultType}");
            }
            else
            {
                _credibleResults.VerifyCredibility(resultType);
                return JsonSerializer.Deserialize(ref reader, resultType, options)
                    ?? throw new MediatorException($"Can not deserialize json to type {resultType}");
            }

        }

        public override void Write(Utf8JsonWriter writer, IMediatorResponse value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteBoolean(nameof(IMediatorResponse.Success), value.Success);

            writer.WritePropertyName(nameof(IMediatorResponse.Results));
            writer.WriteStartArray();
            foreach (var result in value.Results)
            {
                writer.WriteStartObject();
                var resultType = result.GetType();
                writer.WriteString("$type", ContractSerializerTypeHelper.GetIdentifier(resultType));
                if (ContractSerializerTypeHelper.IsEnumerable(resultType))
                {
                    writer.WritePropertyName("Items");
                    JsonSerializer.Serialize(writer, result, resultType, options);
                }
                else
                {
                    ContractSerializerTypeHelper.WriteObjectProperties(writer, result, resultType, options);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}
