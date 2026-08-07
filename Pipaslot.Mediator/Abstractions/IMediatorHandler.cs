using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Abstractions;

/// <summary>
/// Top level action handler marker not returning any data.
/// </summary>
/// <remarks>
/// This is what registration and resolution actually look for. Intermediate interfaces - <see cref="IMessageHandler{TMessage}"/>,
/// or an <c>ICommandHandler&lt;TCommand&gt;</c> of your own - are naming: registration inspects a handler's full interface
/// set and registers it under the closed <see cref="IMediatorHandler{TAction}"/> it ends up implementing, however many
/// layers away that is. A handler is free to implement this interface directly. See
/// docs/wiki/9.1.-Custom-action-and-handler-types.md.
/// <para>
/// Register handlers with <c>AddHandlers</c>/<c>AddHandlersFromAssemblyOf&lt;T&gt;</c> on the configurator returned by
/// <c>AddMediator</c>; they are resolved from the container when the pipeline reaches its execution middleware. Exactly
/// one handler may be registered per action type unless the handlers are marked with <see cref="ISequenceHandler"/> or
/// <see cref="IConcurrentHandler"/> - a second unmarked handler fails the registration check with
/// <see cref="MediatorException"/>. This is not MediatR's <c>INotificationHandler</c>, where every subscriber receives
/// the message.
/// </para>
/// <para>
/// To fail the action, throw: the exception reaches <see cref="IMediator.DispatchUnhandled"/> callers with its original
/// type, and a registered <see cref="IMediatorExceptionHandler{TException}"/> translates it into a message for
/// <see cref="IMediator.Dispatch"/> callers. The alternative is the <see cref="Middlewares.MediatorContext"/> taken from
/// <see cref="IMediatorContextAccessor"/> - <c>AddError</c> for a message written for a user, <c>AddException</c> when an
/// unhandled caller has to branch on the cause. Implement <see cref="Authorization.IHandlerAuthorization{TAction}"/> on
/// the same class to guard the action.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class DeleteUserHandler(IUserRepository repository) : IMediatorHandler&lt;DeleteUser&gt;
/// {
///     public Task Handle(DeleteUser action, CancellationToken cancellationToken)
///     {
///         return repository.Delete(action.Id, cancellationToken);
///     }
/// }
///
/// services.AddMediator()
///     .AddHandlersFromAssemblyOf&lt;DeleteUserHandler&gt;();
/// </code>
/// </example>
/// <typeparam name="TAction">Action type to be processed</typeparam>
public interface IMediatorHandler<in TAction> where TAction : IMediatorAction
{
    /// <summary>Handles an message</summary>
    /// <param name="action">The action to be processed containing input parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Handle(TAction action, CancellationToken cancellationToken);
}

/// <summary>
/// Top level action handler marker returning data in case of successfull execution.
/// </summary>
/// <remarks>
/// The result-returning root, with the same registration and failure rules as <see cref="IMediatorHandler{TAction}"/>,
/// and the same freedom of naming above it - <see cref="IRequestHandler{TRequest,TResponse}"/> or an
/// <c>IQueryHandler&lt;TQuery,TResult&gt;</c> of your own.
/// <para>
/// <see cref="Handle"/> returns the bare <typeparamref name="TResult"/>; the <see cref="IMediatorResponse{TResult}"/>
/// wrapper the caller sees is built by the mediator, so a handler never constructs one. Returning null is legitimate -
/// it is recorded as <see cref="NullActionResult"/> rather than treated as a missing result.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class GetUserHandler(IUserRepository repository) : IMediatorHandler&lt;GetUser, UserDto&gt;
/// {
///     public async Task&lt;UserDto&gt; Handle(GetUser action, CancellationToken cancellationToken)
///     {
///         return await repository.GetUser(action.Id, cancellationToken);
///     }
/// }
/// </code>
/// </example>
/// <typeparam name="TAction">Action type to be processed</typeparam>
/// <typeparam name="TResult">Result type returned by the handler to be provided by mediator</typeparam>
public interface IMediatorHandler<in TAction, TResult> where TAction : IMediatorAction<TResult>
{
    /// <summary>Handles an action and return result</summary>
    /// <param name="action">The action to be processed containing input parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the request</returns>
    Task<TResult> Handle(TAction action, CancellationToken cancellationToken);
}
