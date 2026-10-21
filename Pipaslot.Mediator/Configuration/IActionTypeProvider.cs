using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Configuration;

/// <summary>
/// Source of the action types registered through the mediator configuration. Infrastructure for <see cref="Services.IHandlerExistenceChecker"/>.
/// </summary>
/// <remarks>
/// Not meant to be consumed by applications; use <see cref="Services.IHandlerExistenceChecker"/> to verify registrations.
/// </remarks>
[Obsolete("Infrastructure type that will become internal in 10.0. Resolve IHandlerExistenceChecker from the service provider instead.")]
public interface IActionTypeProvider
{
    /// <summary>Action types registered at startup, both messages and requests.</summary>
    ICollection<Type> GetActionTypes();
    /// <summary>Registered action types that return no result.</summary>
    ICollection<Type> GetMessageActionTypes();
    /// <summary>Registered action types that return a result.</summary>
    ICollection<Type> GetRequestActionTypes();
}