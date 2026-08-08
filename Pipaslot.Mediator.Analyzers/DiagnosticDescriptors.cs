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

    public const string HandlerAuthorizationTypeMismatchId = "PIPMED003";

    public static readonly DiagnosticDescriptor HandlerAuthorizationTypeMismatch = new(
        HandlerAuthorizationTypeMismatchId,
        title: "Handler authorizes a type it does not handle",
        messageFormat: "'{0}' handles '{1}', but its IHandlerAuthorization<T>/IHandlerAuthorizationAsync<T> only covers '{2}'; add IHandlerAuthorization<{1}> (or IHandlerAuthorizationAsync<{1}>), or authorize a shared base type of both actions instead",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "PolicyResolver resolves a handler's Authorize/AuthorizeAsync method by reflection and invokes it with the dispatched action instance, without ever checking that the method's declared type parameter matches an action the handler actually handles. A mismatch is invisible at build and startup time and only surfaces as an ArgumentException from MethodBase.Invoke when the unmatched action is dispatched. Authorizing a shared base type of the handler's actions (including IMediatorAction itself) is a supported pattern and is not flagged.",
        helpLinkUri: "https://github.com/pipaslot/Mediator/wiki/7.-Authorization#keeping-the-authorized-action-in-sync-with-the-handled-action-pipmed003");
}
