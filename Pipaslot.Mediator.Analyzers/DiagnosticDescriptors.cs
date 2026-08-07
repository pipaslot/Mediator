using Microsoft.CodeAnalysis;

namespace Pipaslot.Mediator.Analyzers;

internal static class DiagnosticDescriptors
{
    public const string CatchAllMiddlewareId = "PIPMED001";

    public static readonly DiagnosticDescriptor CatchAllMiddleware = new(
        CatchAllMiddlewareId,
        title: "Middleware swallows a broad exception instead of using an exception handler",
        messageFormat: "'{0}' catches 'Exception' without rethrowing it or recording it via context.AddException; register an IMediatorExceptionHandler<TException> instead so the mediator boundary still observes the exception",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Since safe-by-default exception handling, an IMediatorMiddleware catching a broad exception and converting it into a message hides that exception from the mediator boundary - and therefore from ExecuteUnhandled callers and from the boundary's own logging. Replace the catch block with a typed IMediatorExceptionHandler<TException> registration, or with context.AddException(e) if the middleware needs to keep running its own logic around next() afterwards.",
        helpLinkUri: "https://github.com/pipaslot/Mediator/wiki/6.2.-Exception-handling#migrating-from-a-catch-all-errorhandlingmiddleware");
}
