using Pipaslot.Mediator.Abstractions;
using System;

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

    private readonly HttpGetActionAllowlist _httpGetAllowlist = new();
    

    /// <summary>
    /// Allow an action to be invoked over HTTP GET when <paramref name="condition"/> returns true for it. Registering
    /// any condition enables <see cref="RestrictHttpGetToAllowedActions"/> as a side effect; when multiple conditions
    /// are registered, an action is allowed if at least one of them returns true.
    /// </summary>
    /// <param name="condition">Returns true for actions safe to trigger from a plain hyperlink or embedded resource (e.g. a file download query).</param>
    public ServerMediatorOptions AllowHttpGetWhen(Func<IMediatorAction, bool> condition)
    {
        _httpGetAllowlist.Allow(condition);
        return this;
    }

    /// <summary>
    /// Whether <paramref name="action"/> is allowed to be invoked over HTTP GET: always true when
    /// <see cref="RestrictHttpGetToAllowedActions"/> is disabled, otherwise true when at least one registered
    /// condition returns true for it.
    /// </summary>
    internal bool IsAllowedOverHttpGet(IMediatorAction action)
    {
        return _httpGetAllowlist.IsAllowed(action);
    }

    #endregion
}
