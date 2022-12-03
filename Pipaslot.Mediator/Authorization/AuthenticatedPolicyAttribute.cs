using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class AuthenticatedPolicyAttribute : Attribute, IPolicy
    {
        public async Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            var set = await IdentityPolicy.Authenticated().Resolve(services, cancellationToken);
            return set.SetIdentityStatic();
        }
    }
}
