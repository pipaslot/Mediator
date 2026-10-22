using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Lets only an authenticated user execute the decorated action or handler, i.e. one whose principal from
/// <see cref="IClaimPrincipalAccessor"/> has an authenticated identity.
/// </summary>
/// <remarks>
/// Applies when placed on the action, its handler, or an interface either of them implements. Use
/// <see cref="RolePolicyAttribute"/> to also require a role, or <see cref="IHandlerAuthorization{TAction}"/> when the
/// decision depends on the action's data.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class AuthenticatedPolicyAttribute : Attribute, IPolicy
{
    /// <inheritdoc/>
    public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
    {
        return IdentityPolicy.Authenticated().Resolve(services, cancellationToken);
    }
}