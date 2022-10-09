using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    public interface IPolicy
    {
        public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken);
    }
}
