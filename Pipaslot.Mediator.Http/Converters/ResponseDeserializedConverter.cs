using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Pipaslot.Mediator.Http.FullJsonContractSerializer;

namespace Pipaslot.Mediator.Http.Converters
{
    internal class ResponseDeserializedConverter : JsonConverter<ResponseDeserialized>
    {
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
                            results = ReadResults(ref reader);
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
            var queryType = ContractSerializerTypeHelper.GetType(type);
            return JsonSerializer.Deserialize(content, queryType) ?? throw new Exception($"Can not deserialize json {content} to type {queryType}");
        }

        public override void Write(Utf8JsonWriter writer, ResponseDeserialized value, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"Object {nameof(ResponseDeserialized)} is not pupported to be serialized");
        }
    }
}
