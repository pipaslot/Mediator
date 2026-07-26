using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Http;

public static class MediatorContextExtensions
{
    /// <summary>
    /// Hints the HTTP status code that <see cref="MediatorMiddleware"/> should apply to the root HTTP response,
    /// instead of a middleware/handler writing to <c>HttpContext.Response.StatusCode</c> directly.
    /// <para>
    /// No-op when <paramref name="context"/> is nested (<see cref="MediatorContext.IsNested"/> is <c>true</c>): a
    /// nested mediator call does not own the root HTTP response, so the hint would only take effect if the calling
    /// handler explicitly forwarded it - not a supported pattern for this API. Only call this from a
    /// middleware/handler that is running for the root action.
    /// </para>
    /// <para>
    /// Takes precedence over <see cref="Configuration.ServerMediatorOptions.ErrorHttpStatusCode"/>, but yields to an
    /// <see cref="IMediatorHttpResult"/> present in the root Results - see <see cref="MediatorMiddleware"/>.
    /// </para>
    /// </summary>
    /// <param name="context">Context of the currently processed action.</param>
    /// <param name="statusCode">HTTP status code to apply to the root response, e.g. 400 for a validation error.</param>
    public static void SetResponseStatusCodeHint(this MediatorContext context, int statusCode)
    {
        if (context.IsNested)
        {
            return;
        }

        context.AddResult(new ResponseStatusCodeHint(statusCode));
    }
}
