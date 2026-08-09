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

    public const string IgnoredMediatorResponseId = "PIPMED002";

    public static readonly DiagnosticDescriptor IgnoredMediatorResponse = new(
        IgnoredMediatorResponseId,
        title: "Mediator response is discarded",
        messageFormat: "The IMediatorResponse returned by '{0}' is discarded; check .Success/.Failure before continuing, or call {0}Unhandled instead so a failure throws rather than being silently dropped",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Dispatch and Execute never throw on failure - they report it only through the returned IMediatorResponse/IMediatorResponse<TResult>. A statement-level call whose result is never assigned anywhere silently drops that failure. Either check IMediatorResponse.Success (or .Failure) before continuing, or call DispatchUnhandled/ExecuteUnhandled instead, which throw on failure so it cannot be dropped silently.",
        helpLinkUri: "https://github.com/pipaslot/Mediator/wiki/5.-Mediator-API#discarding-the-response-pipmed002");

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

    public const string MixedAuthorizationMechanismId = "PIPMED005";

    public static readonly DiagnosticDescriptor MixedAuthorizationMechanism = new(
        MixedAuthorizationMechanismId,
        title: "Action/handler pair splits or mixes authorization across more than one place",
        messageFormat: "'{0}' handling '{1}' declares authorization in more than one place ({2}); keep a pair's authorization on a single class, using a single mechanism, instead of splitting or mixing it",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "PolicyResolver ANDs every active authorization source together - IActionAuthorization on the action, IHandlerAuthorization<T>/IHandlerAuthorizationAsync<T> on the handler, and policy attributes (anything implementing IPolicy, e.g. AuthenticatedPolicyAttribute/RolePolicyAttribute) on either - regardless of how many are declared or where, so declaring authorization on both sides of a pair or combining an interface with an attribute on one class both still run correctly. It is a maintainability risk rather than a runtime failure: the effective policy can then only be read by checking every source across however many classes declare one, and a future edit to one source can silently leave another stale. Any number of sources declared on a single class (several attributes, or the interface alone) stay on one class and are not flagged - only a source appearing on both the action and the handler, or an interface mixed with an attribute on the same class, is.",
        helpLinkUri: "https://github.com/pipaslot/Mediator/wiki/7.-Authorization#keep-authorization-for-an-actionhandler-pair-in-one-place-pipmed005");
}
