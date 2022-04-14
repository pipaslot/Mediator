using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Converters
{
    internal class ContractSerializableConverter : JsonConverter<FullJsonContractSerializer.ContractSerializable>
    {
        private readonly ICredibleActionProvider _credibleActions;

        public ContractSerializableConverter(ICredibleActionProvider credibleActions)
        {
            _credibleActions = credibleActions;
        }

        public override FullJsonContractSerializer.ContractSerializable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                        case nameof(FullJsonContractSerializer.ContractSerializable.Type):
                            type = reader.GetString() ?? "";
                            break;
                        case nameof(FullJsonContractSerializer.ContractSerializable.Content):
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

        private FullJsonContractSerializer.ContractSerializable CreateObject(string content, string type)
        {
            var actionType = ContractSerializerTypeHelper.GetType(type);
            _credibleActions.VerifyCredibility(actionType);

            var result = JsonSerializer.Deserialize(content, actionType);
            if (result == null)
            {
                throw new Exception($"Can not deserialize contract as type {type} received from server");
            }
            return new FullJsonContractSerializer.ContractSerializable(result, type);
        }

        public override void Write(Utf8JsonWriter writer, FullJsonContractSerializer.ContractSerializable value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), FullJsonContractSerializer.SerializationOptionsWithoutConverters);
        }
    }
}
