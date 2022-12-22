using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    public interface IPolicy
    {
        public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken);
#if !NETSTANDARD
        public static IPolicy operator &(IPolicy c1, IPolicy c2)
        {
            return c1.And(c2);
        }

        public static IPolicy operator |(IPolicy c1, IPolicy c2)
        {
            return c1.Or(c2);
        }
#endif
    }
}
