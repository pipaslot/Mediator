using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AnonymousPolicyAttribute : Attribute, IPolicy
    {
        public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            return IdentityPolicy.Anonymous().Resolve(services, cancellationToken);
        }
    }
}
