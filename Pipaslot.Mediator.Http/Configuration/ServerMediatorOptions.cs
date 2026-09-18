using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

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
    /// The currently registered HTTP GET allowlist conditions. Registering any condition (via this setter or via
    /// <see cref="AllowHttpGetWhen"/>) restricts HTTP GET to actions allowed by at least one registered condition;
    /// when multiple conditions are registered, an action is allowed if at least one of them returns true.
    /// Setting this property replaces all previously registered conditions wholesale (clear + replace), which lets a
    /// later configuration step fully override or narrow an earlier step's <see cref="AllowHttpGetWhen"/> calls -
    /// e.g. a shared library registers a condition and an application composing it afterwards wants to correct or
    /// undo it. Assigning an empty sequence restores the unrestricted default (every action allowed over HTTP GET).
    /// Mirrors <see cref="BaseMediatorOptions{TBuilder}.CredibleResultTypes"/>, which solves the same
    /// additive-only-vs-replaceable problem for the credible-type allowlist.
    /// There is no separate "is restricted" flag; check <c>HttpGetConditions.Any()</c> when that's needed.
    /// </summary>
    public IEnumerable<Func<IMediatorAction, bool>> HttpGetConditions
    {
        get => _httpGetAllowlist.Conditions;
        set
        {
            _httpGetAllowlist.Clear();
            foreach (var condition in value)
            {
                _httpGetAllowlist.Allow(condition);
            }
        }
    }

    /// <summary>
    /// Allow an action to be invoked over HTTP GET when <paramref name="condition"/> returns true for it. Adds to
    /// any conditions already registered; see <see cref="HttpGetConditions"/> to inspect, replace, or clear them.
    /// </summary>
    /// <param name="condition">Returns true for actions safe to trigger from a plain hyperlink or embedded resource (e.g. a file download query).</param>
    public ServerMediatorOptions AllowHttpGetWhen(Func<IMediatorAction, bool> condition)
    {
        _httpGetAllowlist.Allow(condition);
        return this;
    }

    /// <summary>
    /// Whether <paramref name="action"/> is allowed to be invoked over HTTP GET: always true when no condition has
    /// ever been registered via <see cref="AllowHttpGetWhen"/> or <see cref="HttpGetConditions"/>, otherwise true
    /// when at least one registered condition returns true for it.
    /// </summary>
    internal bool IsAllowedOverHttpGet(IMediatorAction action)
    {
        return _httpGetAllowlist.IsAllowed(action);
    }

    #endregion
}
