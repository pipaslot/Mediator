using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.V2.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.V2.Converters
{
    internal class ResponseDeserializedConverter : JsonConverter<ResponseDeserialized>
    {
        private readonly ICredibleProvider _credibleResults;

        public ResponseDeserializedConverter(ICredibleProvider credibleResults)
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
                        case nameof(ResponseDeserialized.Results):
                            results = ReadResults(ref reader);
                            break;
                    }
                }
            }
            return new ResponseDeserialized
            {
                Success = success,
                Results = results
            };
        }
        private object[] ReadResults(ref Utf8JsonReader reader)
        {
            var results = new List<object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }
                results.Add(ReadResult(ref reader));
            }
            return results.ToArray();
        }

        private object ReadResult(ref Utf8JsonReader reader)
        {
            var type = "";
            var content = "";
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
                        case nameof(ContractSerializable.Type):
                            type = reader.GetString() ?? "";
                            break;
                        case nameof(ContractSerializable.Content):
                            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
                            {
                                content = jsonDoc.RootElement.GetRawText();
                            }
                            break;
                    }
                }
            }
            var resultType = ContractSerializerTypeHelper.GetType(type);
            _credibleResults.VerifyCredibility(resultType);
            return JsonSerializer.Deserialize(content, resultType) ?? throw new MediatorException($"Can not deserialize json {content} to type {resultType}");
        }

        public override void Write(Utf8JsonWriter writer, ResponseDeserialized value, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"Object {nameof(ResponseDeserialized)} is not pupported to be serialized");
        }
    }
}
