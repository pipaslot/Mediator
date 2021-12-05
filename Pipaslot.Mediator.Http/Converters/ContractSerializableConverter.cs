﻿using Pipaslot.Mediator.Configuration;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Pipaslot.Mediator.Http.FullJsonContractSerializer;

namespace Pipaslot.Mediator.Http.Converters
{
    internal class ContractSerializableConverter : JsonConverter<ContractSerializable>
    {
        private readonly PipelineConfigurator _configurator;

        public ContractSerializableConverter(PipelineConfigurator configurator)
        {
            _configurator = configurator;
        }

        public override ContractSerializable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var content = "";
            var type = "";
            string propertyName;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return CreateObject(content, type);
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString() ?? "";
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
            return CreateObject(content, type);
        }

        private ContractSerializable CreateObject(string content, string type)
        {
            var queryType = ContractSerializerTypeHelper.GetType(type);
            if (!_configurator.ActionMarkerAssemblies.Contains(queryType.Assembly))
            {
                throw MediatorHttpException.CreateForUnregisteredType(queryType);
            }
            var result = JsonSerializer.Deserialize(content, queryType);
            if (result == null)
            {
                throw new Exception($"Can not deserialize contract as type {type} received from server");
            }
            return new ContractSerializable(result, type);
        }

        public override void Write(Utf8JsonWriter writer, ContractSerializable value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), SerializationOptionsWithoutConverters);
        }
    }
}
