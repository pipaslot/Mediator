using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Lets only a user with the listed role claims (<see cref="System.Security.Claims.ClaimTypes.Role"/>, compared
/// case-insensitively) execute the decorated action or handler.
/// </summary>
/// <remarks>
/// Applies when placed on the action, its handler, or an interface either of them implements. Equivalent to
/// <see cref="IdentityPolicy.Role"/>; use <see cref="IdentityPolicy.Claim"/> from <see cref="IHandlerAuthorization{TAction}"/>
/// for a claim other than a role.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class RolePolicyAttribute : Attribute, IPolicy
{
    private readonly IdentityPolicy _policy;

    /// <summary>
    /// Requires all of <paramref name="requiredRoles"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="requiredRoles"/> is empty.</exception>
    public RolePolicyAttribute(params string[] requiredRoles) : this(Operator.And, requiredRoles)
    {
    }

    /// <summary>
    /// Requires all of <paramref name="requiredRoles"/> for <see cref="Operator.And"/>, or any of them for <see cref="Operator.Or"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="requiredRoles"/> is empty.</exception>
    public RolePolicyAttribute(Operator @operator, params string[] requiredRoles)
    {
        if (requiredRoles.Length == 0)
        {
            throw new ArgumentException("Can not be empty collection", nameof(requiredRoles));
        }

        _policy = new IdentityPolicy(@operator);
        foreach (var role in requiredRoles)
        {
            _policy.HasRole(role);
        }
    }

    /// <inheritdoc/>
    public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
    {
        return _policy.Resolve(services, cancellationToken);
    }
}