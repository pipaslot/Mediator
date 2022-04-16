using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.Converters
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
            var resultType = ContractSerializerTypeHelper.GetType(typeValue);
            _credibleResults.VerifyCredibility(resultType);
            return JsonSerializer.Deserialize(ref reader, resultType)
                ?? throw new MediatorException($"Can not deserialize json to type {resultType}");
        }

        //private object ReadResult(ref Utf8JsonReader reader)
        //{
        //    var type = "";
        //    var content = "";
        //    while (reader.Read())
        //    {
        //        if (reader.TokenType == JsonTokenType.EndObject)
        //        {
        //            break;
        //        }

        //        if (reader.TokenType == JsonTokenType.PropertyName)
        //        {
        //            var propertyName = reader.GetString() ?? "";
        //            reader.Read();
        //            switch (propertyName)
        //            {
        //                case nameof(ContractSerializable.Type):
        //                    type = reader.GetString() ?? "";
        //                    break;
        //                case nameof(ContractSerializable.Content):
        //                    using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        //                    {
        //                        content = jsonDoc.RootElement.GetRawText();
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //    var resultType = ContractSerializerTypeHelper.GetType(type);
        //    _credibleResults.VerifyCredibility(resultType);
        //    return JsonSerializer.Deserialize(content, resultType) ?? throw new MediatorException($"Can not deserialize json {content} to type {resultType}");
        //}

        public override void Write(Utf8JsonWriter writer, ResponseDeserialized value, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"Object {nameof(ResponseDeserialized)} is not pupported to be serialized");
        }
    }
}
