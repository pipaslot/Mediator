using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// A condition that resolves into a <see cref="RuleSet"/> for the current call: <see cref="Rule"/>, <see cref="RuleSet"/>,
/// <see cref="IdentityPolicy"/>, <see cref="Policy"/> and the policy attributes all implement it.
/// </summary>
/// <remarks>
/// Implement it for a reusable custom policy that needs services to decide, and return it from
/// <see cref="IHandlerAuthorization{TAction}"/>; implement it on an <see cref="Attribute"/> to apply it declaratively, as
/// <see cref="RolePolicyAttribute"/> does. Combine instances with <c>&amp;</c>/<c>|</c> or <see cref="Policy"/>.
/// </remarks>
public interface IPolicy
{
    /// <summary>
    /// Evaluates the policy. <paramref name="services"/> is the service provider of the current mediator call.
    /// </summary>
    public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken);

    /// <summary>
    /// Combines both policies into a new <see cref="Policy"/> with <see cref="Operator.And"/>.
    /// </summary>
    public static IPolicy operator &(IPolicy c1, IPolicy c2)
    {
        return c1.And(c2);
    }

    /// <summary>
    /// Combines both policies into a new <see cref="Policy"/> with <see cref="Operator.Or"/>.
    /// </summary>
    public static IPolicy operator |(IPolicy c1, IPolicy c2)
    {
        return c1.Or(c2);
    }
}