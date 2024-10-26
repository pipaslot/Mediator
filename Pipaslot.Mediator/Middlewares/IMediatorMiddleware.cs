using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Surround handler execution
/// Implementations adds additional behavior and await the next delegate.
/// </summary>
public interface IMediatorMiddleware
{
    /// <summary>
    /// Pipeline handler. Perform any additional behavior and await the <paramref name="next"/> delegate as necessary
    /// </summary>
    /// <param name="context">Outgoing response</param>
    /// <param name="next">Awaitable delegate for the next middleware in the pipeline. Eventually this delegate represents the handler.</param>
    /// <returns>Awaitable task</returns>
    Task Invoke(MediatorContext context, MiddlewareDelegate next);
}