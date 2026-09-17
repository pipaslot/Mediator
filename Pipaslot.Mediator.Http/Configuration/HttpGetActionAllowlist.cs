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

    public bool IsRestricted => _conditions.Count > 0;

    public void Allow(Func<IMediatorAction, bool> condition)
    {
        _conditions.Add(condition);
    }

    public bool IsAllowed(IMediatorAction action)
    {
        return !IsRestricted || _conditions.Any(condition => condition(action));
    }
}
