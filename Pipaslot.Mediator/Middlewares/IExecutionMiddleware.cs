namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Default interface marking middleware as last middleware in pipeline executing handlers or other operations with actions.
/// As service registered in service collection represents default execution middleware used if no pipeline is defined or the pipeline does not specify execution middleware.
/// </summary>
/// <remarks>
/// Terminal by definition: it sits at the end of every pipeline and is what actually resolves and runs the handlers, so
/// it has no meaningful <c>next</c> delegate to await. Exactly one applies to a given action - the one registered in the
/// matching pipeline, or the service-collection registration otherwise.
/// <para>
/// This is the seam the HTTP transport uses: <c>AddMediatorServer</c> keeps the default handler-executing implementation,
/// while <c>AddMediatorClient</c> replaces it with one that serializes the action, calls the server endpoint and
/// deserializes the response - which is why the same action and the same surrounding middlewares work on both sides
/// without the calling code knowing where the handler lives. Implement it to route execution somewhere else (a queue,
/// a different process); implement <see cref="IMediatorMiddleware"/> instead to add behavior around execution.
/// </para>
/// </remarks>
public interface IExecutionMiddleware : IMediatorMiddleware;
