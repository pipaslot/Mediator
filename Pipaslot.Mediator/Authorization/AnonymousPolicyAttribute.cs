using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AnonymousPolicyAttribute : Attribute, IPolicy
    {
        public async Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            var set = await IdentityPolicy.Anonymous().Resolve(services, cancellationToken);
            return set.SetIdentityStatic();
        }
    }
}
