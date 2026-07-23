using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http;

/// <summary>
/// Opt-in contract for handler results that should be applied directly to the HTTP response instead of being
/// serialized as part of the default JSON <see cref="IMediatorResponse"/> body.
/// <para>
/// Only applied by <see cref="MediatorMiddleware"/>, for the root HTTP call, using the result found in the root
/// action's <see cref="Pipaslot.Mediator.Middlewares.MediatorContext"/> Results. A result returned by a nested
/// mediator call (<see cref="Pipaslot.Mediator.Middlewares.MediatorContext.IsNested"/>) is only applied if the
/// calling handler explicitly forwards it as its own result — nested results never leak into the root response
/// automatically, so no nesting check is required by the handler.
/// </para>
/// <para>
/// At most one <see cref="IMediatorHttpResult"/> may be present in the root Results. If more than one is found,
/// <see cref="MediatorMiddleware"/> throws a <see cref="MediatorHttpException"/> instead of silently applying one
/// and discarding the rest.
/// </para>
/// <para>
/// The implementation owns and is responsible for disposing of any resources (streams, etc.) it holds.
/// </para>
/// </summary>
public interface IMediatorHttpResult
{
    Task ApplyAsync(HttpContext httpContext, CancellationToken cancellationToken);
}
