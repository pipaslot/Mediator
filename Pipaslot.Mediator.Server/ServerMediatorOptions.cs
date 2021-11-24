using Pipaslot.Mediator.Serialization;

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
        /// <summary>
        /// If TRUE, server response will be serialized depending on HTTP header passed from client.
        /// Use only if you have Pipaslot.Mediator.Client in version 1.x
        /// </summary>
        public bool KeepCompatibilityWithVersion1 { get; set; }
    }
}
