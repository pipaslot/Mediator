using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.V3.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.V3.Converters
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
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ContractSerializable value, JsonSerializerOptions options)
        {
            MediatorActionConvert.Write(writer, value.Content, options);
        }
    }
}
