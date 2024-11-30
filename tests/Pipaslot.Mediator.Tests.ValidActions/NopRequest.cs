using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions
{
    public class NopRequest : IRequest<string>
    {
    }

    public class NopRequestHandler : IRequestHandler<NopRequest, string>
    {
        public Task<string> Handle(NopRequest action, CancellationToken cancellationToken)
        {
            return Task.FromResult("");
        }
    }
}