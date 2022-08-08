using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    public interface IPolicy
    {
        public Task<IRuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken);
    }
}
