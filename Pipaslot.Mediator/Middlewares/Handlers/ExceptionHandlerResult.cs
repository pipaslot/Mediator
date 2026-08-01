namespace Pipaslot.Mediator.Middlewares.Handlers;

/// <summary>
/// Outcome of resolving and invoking a typed <see cref="IMediatorExceptionHandler{TException}"/> for a caught exception.
/// </summary>
internal readonly struct ExceptionHandlerResult
{
    /// <summary>
    /// True when a handler was resolved and invoked without itself throwing.
    /// False when no handler matched, the matched handler could not be resolved from DI, or the handler itself threw -
    /// all of which degrade to the same "no match" outcome so the caller can fall back to the default translation.
    /// </summary>
    public bool IsHandled { get; }

    /// <summary>
    /// The client-facing message produced by the handler. Only meaningful when <see cref="IsHandled"/> is true.
    /// </summary>
    public string? Message { get; }

    private ExceptionHandlerResult(bool isHandled, string? message)
    {
        IsHandled = isHandled;
        Message = message;
    }

    public static readonly ExceptionHandlerResult NotHandled = new(false, null);

    public static ExceptionHandlerResult Handled(string? message) => new(true, message);
}
