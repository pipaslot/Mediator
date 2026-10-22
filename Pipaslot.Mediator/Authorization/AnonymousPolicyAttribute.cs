using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Lets every caller execute the decorated action or handler, including an unauthenticated one.
/// </summary>
/// <remarks>
/// Marks an action as deliberately public: an action whose action type and handlers declare no policy at all is refused
/// with <see cref="AuthorizationException"/> (<see cref="AuthorizationExceptionTypes.NoAuthorization"/>). Applies when placed
/// on the action, its handler, or an interface either of them implements. Use <see cref="AuthenticatedPolicyAttribute"/> or
/// <see cref="RolePolicyAttribute"/> to restrict access, or <see cref="IHandlerAuthorization{TAction}"/> when the decision
/// depends on the action's data.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class AnonymousPolicyAttribute : Attribute, IPolicy
{
    /// <inheritdoc/>
    public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
    {
        return IdentityPolicy.Anonymous().Resolve(services, cancellationToken);
    }
}