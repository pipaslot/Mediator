using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Http.Configuration;

/// <summary>
/// Holds the predicate-based HTTP GET allowlist for <see cref="ServerMediatorOptions"/> and decides whether a given
/// action may be invoked over HTTP GET. Kept as its own object rather than fields on <see cref="ServerMediatorOptions"/>
/// itself, mirroring how <see cref="CredibleActionProvider"/> keeps the analogous "is this type credible" decision off
/// the options object.
/// </summary>
internal class HttpGetActionAllowlist
{
    private readonly List<Func<IMediatorAction, bool>> _conditions = [];

    /// <summary>
    /// The currently registered conditions, in registration order.
    /// </summary>
    public IEnumerable<Func<IMediatorAction, bool>> Conditions => _conditions;

    public void Allow(Func<IMediatorAction, bool> condition)
    {
        _conditions.Add(condition);
    }

    /// <summary>
    /// Removes all registered conditions, restoring the unrestricted default (every action allowed over HTTP GET).
    /// </summary>
    public void Clear()
    {
        _conditions.Clear();
    }

    public bool IsAllowed(IMediatorAction action)
    {
        var isRestricted = _conditions.Count > 0;// TODO in version 10: restrict the filtering by default. The GET method won't be allowed by default
        return !isRestricted || _conditions.Any(condition => condition(action));
    }
}
