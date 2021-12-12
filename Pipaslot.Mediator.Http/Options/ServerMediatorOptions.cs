namespace Pipaslot.Mediator.Http.Options
{
    public class ServerMediatorOptions
    {
        private string _endpoint = MediatorConstants.Endpoint;

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
