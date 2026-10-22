using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Services;

/// <summary>
/// Selects the checks <see cref="IHandlerExistenceChecker.Verify"/> runs. Every check is off by default.
/// </summary>
public class ExistenceCheckerSetting
{
    /// <summary>
    /// Requires every action to have a handler, unless the action carries <see cref="Abstractions.NoHandlerAttribute"/>,
    /// and rejects duplicate handlers and combinations of single, sequence and concurrent handlers for one action.
    /// </summary>
    public bool CheckMatchingHandlers { get; set; }
    /// <summary>
    /// Requires every action to be covered by an authorization policy, declared on the action or on its handlers.
    /// Actions listed in <see cref="IgnoredPolicyChecks"/> are skipped.
    /// </summary>
    public bool CheckExistingPolicies { get; set; }
    /// <summary>
    /// Action types exempt from <see cref="CheckExistingPolicies"/>, matched by exact type.
    /// </summary>
    public HashSet<Type> IgnoredPolicyChecks { get; set; } = [];
}