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
    /// Whether <see cref="MediatorMiddleware"/> restricts which actions it accepts over HTTP GET to the ones allowed by
    /// a condition registered via <see cref="AllowHttpGetWhen"/>/<see cref="AllowHttpGetWhenAction{TAction}"/>. Computed
    /// from whether any condition has been registered - there is no independent flag to set.
    /// </summary>
    /// <remarks>
    /// A GET request can be triggered cross-site without a preflight check and without the caller's consent - an
    /// <c>&lt;img&gt;</c> or <c>&lt;link&gt;</c> tag pointing at the mediator endpoint is enough to make the victim's
    /// browser send it, cookies/Windows authentication included. This is a CSRF risk for any action that changes state.
    /// GET support exists for file downloads and similar read-only scenarios (see
    /// docs/wiki/9.3.-Custom-HTTP-responses-and-file-download.md) where the URL needs to be embeddable in an
    /// <c>&lt;a&gt;</c>/<c>&lt;img&gt;</c> tag; state-changing actions should not opt in.
    /// <para>
    /// With no condition ever registered, every action registered with the mediator can still be invoked over GET,
    /// unchanged from previous versions - this default keeps the fix backward compatible. It is planned that the next
    /// major version will instead deny every action over GET while no condition is registered; until then, register a
    /// condition for the actions (typically none, or only download-style queries) that are safe to trigger from a
    /// plain hyperlink or embedded resource.
    /// </para>
    /// </remarks>
    public bool RestrictHttpGetToAllowedActions => _httpGetAllowlist.IsRestricted;

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
    /// Allow every action implementing <typeparamref name="TAction"/> to be invoked over HTTP GET. Shortcut for
    /// <see cref="AllowHttpGetWhen"/> filtering by type.
    /// </summary>
    /// <typeparam name="TAction">Action marker type safe to trigger from a plain hyperlink or embedded resource.</typeparam>
    public ServerMediatorOptions AllowHttpGetWhenAction<TAction>() where TAction : IMediatorAction
    {
        return AllowHttpGetWhen(a => typeof(TAction).IsAssignableFrom(a.GetType()));
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
