namespace Pipaslot.Mediator.Http.Configuration;

/// <summary>
/// Options of the server side of the HTTP transport, passed to
/// <see cref="ServiceCollectionExtensions.AddMediatorServer(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Action{ServerMediatorOptions})"/>.
/// </summary>
/// <remarks>
/// The server deserializes actions that arrive from outside the trust boundary, so it keeps the credible-type allowlist
/// enabled by default (<see cref="DeserializeOnlyCredibleActionTypes"/>) and lets an action run over HTTP GET only when
/// <see cref="HttpGetConditions"/> allows it. The client counterpart is <see cref="ClientMediatorOptions"/>.
/// </remarks>
public class ServerMediatorOptions : BaseMediatorOptions<ServerMediatorOptions>
{
    /// <summary>
    /// Protect deserialization process by check whether target type is credible.
    /// Prevents agains exploiting this feature by attackers. Enabled by default.
    /// </summary>
    /// <remarks>
    /// While enabled, two conditions must both hold for every type created from an incoming payload. First, when any
    /// action assembly was registered through <c>AddActionsFromAssemblyOf&lt;T&gt;</c> on the configurator, the type must
    /// belong to one of those assemblies. Second, the type must be an action, or be added via
    /// <see cref="BaseMediatorOptions{TBuilder}.AddCredibleResultType{T}"/> or
    /// <see cref="BaseMediatorOptions{TBuilder}.AddCredibleResultAssemblyOf{T}"/> (also as an item of a collection).
    /// The credible additions therefore do not widen the first condition: a contract type from another assembly needs
    /// that assembly registered as an action assembly too. A rejected request fails with
    /// <see cref="MediatorHttpException"/>; fix that by registering the missing assembly or type. Turning the
    /// flag off removes the check, so a caller can name any type for the server to instantiate while deserializing
    /// - an exposure to deserialization attacks, not a fix for a rejected action.
    /// </remarks>
    public bool DeserializeOnlyCredibleActionTypes { get; set; } = true;

    /// <summary>
    /// HTTP Status code returned in server response when mediator returns any error
    /// </summary>
    /// <remarks>
    /// Applied only while the response status is still 200; a status code set through
    /// <see cref="MediatorContextExtensions.SetResponseStatusCodeHint"/> takes precedence. Defaults to
    /// <see cref="MediatorConstants.ErrorHttpStatusCode"/>.
    /// </remarks>
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
