namespace Pipaslot.Mediator.Http.Configuration;

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

    #region HTTP GET allowlist

    /// <summary>
    /// The HTTP GET allowlist: register conditions via <see cref="HttpGetActionAllowlist.Allow"/> deciding whether a
    /// given action instance may be invoked over HTTP GET; when multiple conditions are registered, an action is
    /// allowed if at least one of them returns true. Empty by default, which keeps every action registered with the
    /// mediator invocable over GET (backward compatible). Call <see cref="HttpGetActionAllowlist.Clear"/> to remove
    /// all previously registered conditions and restore that unrestricted default - e.g. when a later configuration
    /// step wants to correct or narrow conditions registered by an earlier one.
    /// </summary>
    public HttpGetActionAllowlist HttpGetConditions { get; } = new();

    #endregion
}
