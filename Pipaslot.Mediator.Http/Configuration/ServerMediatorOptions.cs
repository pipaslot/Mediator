namespace Pipaslot.Mediator.Http.Configuration
{
    public class ServerMediatorOptions : BaseMediatorOptions<ServerMediatorOptions>
    {
        /// <summary>
        /// Protect deserialization process by check whether target type is credible. 
        /// Prevents agains exploiting this feature by attackers. Enabled by default.
        /// </summary>
        public bool DeserializeOnlyCredibleActionTypes { get; set; } = true;

        /// <summary>
        /// HTTP Status code returned in server response when mediator returns any error
        /// </summary>
        public int ErrorHttpStatusCode { get; set; } = MediatorConstants.ErrorHttpStatusCode;

    }
}
