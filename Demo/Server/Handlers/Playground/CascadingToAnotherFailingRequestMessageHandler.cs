using Demo.Server.MediatorMiddlewares;
using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground;

public class CascadingToAnotherFailingRequestMessageHandler(IMediator mediator) : IMessageHandler<CascadingToAnotherFailingRequestMessage>
{
    public async Task Handle(CascadingToAnotherFailingRequestMessage action, CancellationToken cancellationToken)
    {
        try
        {
            await mediator.DispatchUnhandled(new FailingOnValidation.Request(), cancellationToken);
        }
        catch (ValidationException e)
        {
            //ValidatorMiddleware records validation failures via context.AddException, so DispatchUnhandled rethrows
            //this exact typed exception here instead of a generic MediatorUnhandledErrorException.
            var _ = e.Errors;
            throw;
        }
    }
}