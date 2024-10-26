using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class AuthenticatedPolicyAttribute : Attribute, IPolicy
{
    public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
    {
        return IdentityPolicy.Authenticated().Resolve(services, cancellationToken);
    }
}