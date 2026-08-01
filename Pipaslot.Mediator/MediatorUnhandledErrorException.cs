using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator;

/// <summary>
/// Thrown when a handler or middleware set the ExecutionStatus to Failed without an original exception to rethrow.
/// </summary>
public class MediatorUnhandledErrorException(string message, MediatorContext? context) : MediatorExecutionException(message, context)
{
    internal static MediatorUnhandledErrorException Create(MediatorContext context)
    {
        return Create($"'{GetErrors(context.Results)}'", context);
    }
    
    internal static MediatorUnhandledErrorException Create(string errors, MediatorContext context)
    {
        return new MediatorUnhandledErrorException(
            $"Handler or middlewares set the ExecutionStatus to {ExecutionStatus.Failed}. To prevent this exception, user methods Mediator.Dispatch or Mediator.Execute instead. Error messages: [{errors}]",
            context);
    }
}
