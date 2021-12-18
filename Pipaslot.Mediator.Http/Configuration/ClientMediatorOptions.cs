namespace Pipaslot.Mediator.Http.Configuration
{
    public class ClientMediatorOptions
    {
        public string Endpoint { get; set; } = MediatorConstants.Endpoint;
        /// <summary>
        /// Protect deserialization process by check whether target type is credible. 
        /// Prevents agains exploiting this feature by attackers. Disabled by default.
        /// </summary>
        public bool DeserializeOnlyCredibleResultTypes { get; set; } = false;
    }
}
