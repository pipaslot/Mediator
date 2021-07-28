using Pipaslot.Mediator.Contracts;

namespace Pipaslot.Mediator.Server
{
    public class ServerMediatorOptions
    {
        private string endpoint = MediatorRequestSerializable.Endpoint;

        public string Endpoint
        {
            get => endpoint; set
            {
                var notNulValue = (value ?? "").Trim();
                endpoint = notNulValue.StartsWith("/") ? notNulValue : $"/{notNulValue}";
            }
        }
    }
}
