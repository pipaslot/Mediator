using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Http.Configuration;

/// <summary>
/// Holds the predicate-based HTTP GET allowlist exposed via <see cref="ServerMediatorOptions.HttpGetConditions"/> and
/// decides whether a given action may be invoked over HTTP GET. Kept as its own object rather than fields on
/// <see cref="ServerMediatorOptions"/> itself, mirroring how <see cref="CredibleActionProvider"/> keeps the analogous
/// "is this type credible" decision off the options object.
/// </summary>
/// <remarks>
/// Enumerating the allowlist yields the currently registered conditions, in registration order.
/// <para>
/// Protects against cross-site request forgery: a GET request can be triggered by a link, an image tag or a redirect on
/// another site without the user's consent, so an action that changes state must not be reachable that way. Allow only
/// actions that are safe to repeat and have no side effects (typically queries and file downloads); POST requests are
/// not affected. Until the first condition is registered every action is allowed over GET for backward compatibility,
/// so register at least one condition to enable the restriction.
/// </para>
/// </remarks>
public class HttpGetActionAllowlist : IEnumerable<Func<IMediatorAction, bool>>
{
    private readonly List<Func<IMediatorAction, bool>> _conditions = [];

    /// <inheritdoc />
    public IEnumerator<Func<IMediatorAction, bool>> GetEnumerator() => _conditions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Allow an action to be invoked over HTTP GET when <paramref name="condition"/> returns true for it. Adds to
    /// any conditions already registered; when multiple conditions are registered, an action is allowed if at least
    /// one of them returns true.
    /// </summary>
    /// <param name="condition">Returns true for actions safe to trigger from a plain hyperlink or embedded resource (e.g. a file download query).</param>
    public HttpGetActionAllowlist Allow(Func<IMediatorAction, bool> condition)
    {
        _conditions.Add(condition);
        return this;
    }

    /// <summary>
    /// Removes all registered conditions, restoring the unrestricted default (every action allowed over HTTP GET).
    /// </summary>
    public HttpGetActionAllowlist Clear()
    {
        _conditions.Clear();
        return this;
    }

    /// <summary>
    /// Whether <paramref name="action"/> is allowed to be invoked over HTTP GET: always true when no condition has
    /// ever been registered via <see cref="Allow"/>, otherwise true when at least one registered condition returns
    /// true for it. Called by <c>MediatorMiddleware</c> to decide whether to reject an incoming GET request.
    /// </summary>
    public bool IsAllowed(IMediatorAction action)
    {
        var isRestricted = _conditions.Count > 0;// TODO in version 10: restrict the filtering by default. The GET method won't be allowed by default
        return !isRestricted || _conditions.Any(condition => condition(action));
    }
}
