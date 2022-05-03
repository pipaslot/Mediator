using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    public interface IPolicy
    {
        public Task<IEnumerable<Rule>> Resolve(IServiceProvider services, CancellationToken cancellationToken);
    }
}
