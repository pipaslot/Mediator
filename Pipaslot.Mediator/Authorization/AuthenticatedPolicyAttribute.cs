using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AuthenticatedPolicyAttribute : Attribute, IPolicy
    {
        public Task<IEnumerable<Rule>> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            return IdentityPolicy.Authenticated().Resolve(services, cancellationToken);
        }
    }
}
