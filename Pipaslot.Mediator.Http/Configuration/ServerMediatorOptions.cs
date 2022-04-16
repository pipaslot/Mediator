namespace Pipaslot.Mediator.Http.Configuration
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

        /// <summary>
        /// Protect deserialization process by check whether target type is credible. 
        /// Prevents agains exploiting this feature by attackers.Enabled by default.
        /// </summary>
        public bool DeserializeOnlyCredibleActionTypes { get; set; } = true;

        /// <summary>
        /// HTTP Status code returned in server response when mediator returns any error
        /// </summary>
        public int ErrorHttpStatusCode { get; set; } = MediatorConstants.ErrorHttpStatusCode;

        public SerializerType SerializerTyoe { get; set; } = SerializerType.V2;
    }
}
