using Pipaslot.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator;

/// <summary>
///     Request / Message dispatched
/// </summary>
/// <remarks>
/// Four entry points, two axes: <see cref="Execute{TResult}"/>/<see cref="ExecuteUnhandled{TResult}"/> for actions
/// returning data (<see cref="IRequest{TResponse}"/>), <see cref="Dispatch"/>/<see cref="DispatchUnhandled"/> for those
/// that do not (<see cref="IMessage"/>); the plain pair reports failures through the returned
/// <see cref="IMediatorResponse"/>, the <c>*Unhandled</c> pair throws instead. There is no MediatR-style <c>Send</c>.
/// <para>
/// Prefer <see cref="Execute{TResult}"/>/<see cref="Dispatch"/> at UI or API boundaries, where a failure is a message to
/// show; prefer the <c>*Unhandled</c> pair inside code that cannot meaningfully continue after a failure - typically a
/// nested call from another handler, where letting the exception travel up is the point.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Failure reported through the response
/// var response = await mediator.Execute(new GetUser(42));
/// if (response.Failure)
/// {
///     return BadRequest(response.GetErrorMessage());
/// }
/// UserDto user = response.Result;
///
/// // Failure reported by an exception
/// UserDto sameUser = await mediator.ExecuteUnhandled(new GetUser(42));
/// </code>
/// </example>
public interface IMediator
{
    /// <summary>
    /// Execute action and wait for response data
    /// </summary>
    /// <remarks>
    /// Does not throw when the action fails - the failure is reported through the returned wrapper, so
    /// <see cref="IMediatorResponse.Success"/> (or <see cref="IMediatorResponse.Failure"/>) must be checked before
    /// reading <see cref="IMediatorResponse{TResult}.Result"/>, which is <c>default</c> on failure. Note that the result
    /// is a wrapper and not the bare <typeparamref name="TResult"/> that MediatR's <c>Send</c> returns; for the bare
    /// value use <see cref="ExecuteUnhandled{TResult}"/>. An exception raised inside the pipeline is caught here,
    /// logged, and turned into an error message - either the generic one, or the message produced by a registered
    /// <see cref="IMediatorExceptionHandler{TException}"/>. Only a null <paramref name="request"/> throws.
    /// </remarks>
    /// <typeparam name="TResult">Result object type returned from handler</typeparam>
    /// <param name="request">Object managing input parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns wrapper managing response state and data</returns>
    Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send message without feedback expectation
    /// </summary>
    /// <remarks>
    /// The <see cref="IMessage"/> counterpart of <see cref="Execute{TResult}"/> and subject to the same rule: it does not
    /// throw when the action fails, so a caller that ignores the returned <see cref="IMediatorResponse"/> silently
    /// ignores the failure too. Use <see cref="DispatchUnhandled"/> when there is nothing sensible to do with the
    /// response. "Without feedback expectation" refers to result data only - error messages and notifications still come
    /// back through <see cref="IMediatorResponse.Results"/>.
    /// </remarks>
    /// <param name="message">Object managing input parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns wrapper managing response state</returns>
    Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute action and wait for response data. Exception will be thrown if processing was unsuccessfull.
    /// </summary>
    /// <remarks>
    /// Returns the bare <typeparamref name="TResult"/> - the closest equivalent of MediatR's <c>Send</c>. Failures are not
    /// translated into a client-safe message here: an exception recorded during the pipeline is rethrown with its original
    /// type, and a failure reported only as an error message surfaces as <see cref="MediatorUnhandledErrorException"/>.
    /// Registered <see cref="IMediatorExceptionHandler{TException}"/> instances do not apply to this path. Use
    /// <see cref="Execute{TResult}"/> at a boundary where the failure has to be shown rather than propagated.
    /// Called on the HTTP client, every server-side failure arrives as <see cref="MediatorUnhandledErrorException"/>
    /// instead of its original type - see that type's remarks.
    /// </remarks>
    /// <typeparam name="TResult">Result object type returned from handler</typeparam>
    /// <param name="request">Object managing input parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns data from handler</returns>
    /// <exception cref="MediatorExecutionException">
    /// Action failed, no handler was found (<see cref="MediatorNoHandlerFoundException"/>) or the handler produced no
    /// result of type <typeparamref name="TResult"/> (<see cref="MediatorMissingResultException"/>).
    /// </exception>
    Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send message without feedback expectation. Exception will be thrown if processing was unsuccessfull.
    /// </summary>
    /// <remarks>
    /// The <see cref="IMessage"/> counterpart of <see cref="ExecuteUnhandled{TResult}"/>, and the safest default for a
    /// caller that would otherwise drop the response of <see cref="Dispatch"/> on the floor. Exceptions recorded during
    /// the pipeline are rethrown with their original type, bypassing the registered
    /// <see cref="IMediatorExceptionHandler{TException}"/> instances - except on the HTTP client, where a server-side
    /// failure always surfaces as <see cref="MediatorUnhandledErrorException"/>.
    /// </remarks>
    /// <param name="message">Object managing input parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Does not returns either data nor state</returns>
    /// <exception cref="MediatorExecutionException">
    /// Action failed, or no handler was found (<see cref="MediatorNoHandlerFoundException"/>).
    /// </exception>
    Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default);
}
