using Pipaslot.Mediator.Http;

namespace Pipaslot.Mediator.Server
{
    public class ServerMediatorOptions
    {
        private string _endpoint = Constants.Endpoint;

        public string Endpoint
        {
            get => _endpoint; set
            {
                var notNulValue = (value ?? "").Trim();
                _endpoint = notNulValue.StartsWith("/") ? notNulValue : $"/{notNulValue}";
            }
        }
    }
}
