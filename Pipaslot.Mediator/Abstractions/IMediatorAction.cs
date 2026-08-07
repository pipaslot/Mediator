namespace Pipaslot.Mediator.Abstractions;

/// <summary>
/// Top level action marker for action without returning data. Connects actions returning result with those not not returning data to be processed by Mediator.
/// </summary>
/// <remarks>
/// This is what the mediator actually dispatches on. Everything above it - <see cref="IMessage"/>, or a marker of your own
/// such as <c>ICommand</c> - is naming, and the pipeline never looks at it: an action is recognized by implementing this
/// interface, whether directly or through any number of intermediate interfaces. Define your own vocabulary whenever
/// Request/Message does not fit the domain; see docs/wiki/9.1.-Custom-action-and-handler-types.md.
/// <para>
/// An action is a DTO carrying the input parameters and no behavior - over the HTTP transport it is serialized as-is, so
/// keep it free of services and non-serializable state. It is delivered to a single handler, which makes it the "command"
/// of this library and not MediatR's <c>INotification</c>; for fan-out mark the handlers with <see cref="ISequenceHandler"/>
/// or <see cref="IConcurrentHandler"/>. Dispatch it with <see cref="IMediator.Dispatch"/>, which reports failures through
/// the returned <see cref="IMediatorResponse"/> rather than by throwing, or with <see cref="IMediator.DispatchUnhandled"/>
/// to have failures interrupt the flow. Use <see cref="IMediatorAction{TResult}"/> when the caller needs data back.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Built-in naming
/// public record DeleteUser(int Id) : IMessage;
///
/// // Or your own, equivalent as far as the mediator is concerned
/// public interface ICommand : IMediatorAction;
/// public record DeleteUser(int Id) : ICommand;
///
/// var response = await mediator.Dispatch(new DeleteUser(42));
/// if (response.Failure)
/// {
///     logger.LogWarning(response.GetErrorMessage());
/// }
/// </code>
/// </example>
public interface IMediatorAction;

/// <summary>
/// Top level action marker for action which returns data. All derived types can have own specific pipelines and handlers.
/// </summary>
/// <remarks>
/// The result-carrying root, dispatched through <see cref="IMediator.Execute{TResult}"/>. As with
/// <see cref="IMediatorAction"/>, the marker above it - <see cref="IRequest{TResponse}"/> or a <c>IQuery&lt;TResult&gt;</c>
/// of your own - is naming only.
/// <para>
/// Despite the shared vocabulary this is not MediatR's <c>IRequest&lt;TResponse&gt;</c>:
/// <see cref="IMediator.Execute{TResult}"/> returns <see cref="IMediatorResponse{TResult}"/> - a wrapper carrying the
/// success state, the collected results and any error messages - not a bare <typeparamref name="TResult"/>. Read
/// <see cref="IMediatorResponse.Success"/> before touching <see cref="IMediatorResponse{TResult}.Result"/>, or call
/// <see cref="IMediator.ExecuteUnhandled{TResult}"/> to get the bare value and an exception on failure.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Built-in naming
/// public record GetUser(int Id) : IRequest&lt;UserDto&gt;;
///
/// // Or your own
/// public interface IQuery&lt;TResult&gt; : IMediatorAction&lt;TResult&gt;;
/// public record GetUser(int Id) : IQuery&lt;UserDto&gt;;
///
/// var response = await mediator.Execute(new GetUser(42));
/// if (response.Success)
/// {
///     UserDto user = response.Result;
/// }
/// </code>
/// </example>
/// <typeparam name="TResult">Result data returned from handler execution</typeparam>
public interface IMediatorAction<out TResult> : IMediatorAction, IMediatorActionProvidingData;

/// <summary>
/// FOR INTERNAL PURPOSE ONLY
/// </summary>
public interface IMediatorActionProvidingData;
