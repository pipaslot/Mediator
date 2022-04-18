using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.V3.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.V3.Converters
{
    internal class SimpleResponseDeserializedConverter : JsonConverter<ResponseDeserialized>
    {
        private readonly ICredibleResultProvider _credibleResults;

        public SimpleResponseDeserializedConverter(ICredibleResultProvider credibleResults)
        {
            _credibleResults = credibleResults;
        }

        public override ResponseDeserialized? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                        case nameof(ResponseDeserialized.Success):
                            success = reader.GetBoolean();
                            break;
                        case nameof(ResponseDeserialized.ErrorMessages):
                            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
                            {
                                errorMessages = JsonSerializer.Deserialize<string[]>(jsonDoc.RootElement.GetRawText()) ?? new string[0];
                            }
                            break;
                        case nameof(ResponseDeserialized.Results):
                            results = ReadResults(ref reader, options);
                            break;
                    }
                }
            }
            return new ResponseDeserialized
            {
                Success = success,
                ErrorMessages = errorMessages,
                Results = results
            };
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
                results.Add(ReadResult(ref reader, options/*, _credibleResults, out var _*/));
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
            _credibleResults.VerifyCredibility(resultType);
            if (resultType.IsArray || ContractSerializerTypeHelper.IsEnumerable(resultType))
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
                reader.Read();
                reader.Read();
                reader.Read();
                return JsonSerializer.Deserialize(ref reader, resultType)
                    ?? throw new MediatorException($"Can not deserialize json to type {resultType}");
            }
            else
            {
                return JsonSerializer.Deserialize(ref reader, resultType)
                    ?? throw new MediatorException($"Can not deserialize json to type {resultType}");
            }

        }

        public override void Write(Utf8JsonWriter writer, ResponseDeserialized value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteBoolean(nameof(IMediatorResponse.Success), value.Success);

            writer.WritePropertyName(nameof(IMediatorResponse.ErrorMessages));
            writer.WriteStartArray();
            foreach (var message in value.ErrorMessages)
            {
                writer.WriteStringValue(message);
            }
            writer.WriteEndArray();

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
                    //TODO Optimize
                    using var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(result, resultType, options));
                    foreach (var element in jsonDocument.RootElement.EnumerateObject())
                    {
                        element.WriteTo(writer);
                    }
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
        private void WriteObjectProperties()
        {

        }
    }
}
