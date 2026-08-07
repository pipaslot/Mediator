using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Surround handler execution
/// Implementations adds additional behavior and await the next delegate.
/// </summary>
/// <remarks>
/// The same shape as an ASP.NET Core middleware, with <see cref="MediatorContext"/> in place of <c>HttpContext</c>:
/// register it with <c>Use&lt;TMiddleware&gt;()</c> on the configurator, do work before and after awaiting
/// <c>next(context)</c>, and stop the pipeline by simply not awaiting it - a middleware that returns without calling
/// <c>next</c> prevents the handler (and every middleware behind it) from running at all. Order of registration is order
/// of execution; the terminal position always belongs to an <see cref="IExecutionMiddleware"/>.
/// <para>
/// Instances are resolved from the container per registration lifetime (scoped by default), so constructor injection is
/// available. Fail the action through the context rather than by throwing - a thrown exception is caught at the mediator
/// boundary and reported as a generic message unless a matching <see cref="IMediatorExceptionHandler{TException}"/> is
/// registered. A middleware wrapping <c>next</c> in a try/catch to convert exceptions into messages should be replaced by
/// such a handler.
/// </para>
/// <para>
/// Two ways to fail it, differing in what an <see cref="IMediator.ExecuteUnhandled{TResult}"/> caller receives:
/// <c>context.AddError(...)</c> adds a message for the user and that caller gets a
/// <see cref="MediatorUnhandledErrorException"/> carrying only the text, while <c>context.AddException(...)</c> preserves
/// an exception that the same caller gets back with its original type, without it reaching
/// <see cref="IMediator.Dispatch"/>/<see cref="IMediator.Execute{TResult}"/> callers. Use the first for messages written
/// for a user, the second when calling code has to branch on the cause; both set
/// <see cref="MediatorContext.Status"/> to <see cref="ExecutionStatus.Failed"/> and both can be used together.
/// See docs/wiki/6.-Pipelines-and-Middlewares.md and docs/wiki/6.2.-Exception-handling.md.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class TimingMiddleware(ILogger&lt;TimingMiddleware&gt; logger) : IMediatorMiddleware
/// {
///     public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
///     {
///         var stopwatch = Stopwatch.StartNew();
///         await next(context);
///         logger.LogInformation("{Action} took {Elapsed}", context.ActionIdentifier, stopwatch.Elapsed);
///     }
/// }
///
/// services.AddMediator()
///     .Use&lt;TimingMiddleware&gt;();
/// </code>
/// </example>
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
