using Demo.Shared.Playground;

namespace Demo.Server.Handlers.Playground
{
    public class CustomInternalRequestHandler : IInternalRequestHandler<CustomInternalRequest, bool>
    {
        public Task<bool> Handle(CustomInternalRequest action, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
