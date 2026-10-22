using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator;

/// <summary>
/// Shorthands over <see cref="IMediator.Execute{TResult}"/> for callers that need no error details, and the entry points
/// of authorization checks that do not execute the action.
/// </summary>
public static class MediatorExtensions
{
    /// <summary>
    /// Execute action and return result if operation succeeded otherwise returns default value for the result type
    /// </summary>
    /// <remarks>
    /// The failure itself, including its error messages, is discarded - use <see cref="IMediator.Execute{TResult}"/> when
    /// the caller has to show or log it.
    /// </remarks>
    /// <typeparam name="TResult">Action result type</typeparam>
    /// <returns>Action result type or its default value</returns>
    public static async Task<TResult?> ExecuteOrDefault<TResult>(this IMediator mediator, IMediatorAction<TResult> request,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Execute(request, cancellationToken).ConfigureAwait(false);
        return response.Success ? response.Result : default;
    }

    /// <summary>
    /// Execute action and return result if operation succeeded otherwise returns default value passed as second argument
    /// </summary>
    /// <typeparam name="TResult">Action result type</typeparam>
    /// <returns>Action result type or default value</returns>
    public static async Task<TResult> ExecuteOrDefault<TResult>(this IMediator mediator, IMediatorAction<TResult> request, TResult defaultValue,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Execute(request, cancellationToken).ConfigureAwait(false);
        return response.Success ? response.Result : defaultValue;
    }

    /// <summary>
    /// Execute action and return result if operation succeeded otherwise returns new instance of expected result type via parameterless constructor
    /// </summary>
    /// <remarks>
    /// A new instance is returned also when the action succeeds with a null result.
    /// </remarks>
    /// <typeparam name="TResult">Action result type</typeparam>
    /// <returns>Action result type of its new implementation</returns>
    public static async Task<TResult> ExecuteOrNew<TResult>(this IMediator mediator, IMediatorAction<TResult> request,
        CancellationToken cancellationToken = default)
        where TResult : new()
    {
        var response = await mediator.Execute(request, cancellationToken).ConfigureAwait(false);
        return response.Success
            ? response.Result ?? new TResult()
            : new TResult();
    }

    /// <summary>
    /// Evaluates the authorization policies of <paramref name="action"/> for the current user without executing it -
    /// typically to decide whether to show a button.
    /// </summary>
    /// <returns>
    /// True only when access is <see cref="AccessType.Allow"/>; false also when the check itself failed. Use
    /// <see cref="Authorize"/> to tell a denial from an unavailable check and to get the reason.
    /// </returns>
    public static async Task<bool> IsAuthorized(this IMediator mediator, IMediatorAction action, CancellationToken cancellationToken = default)
    {
        var response = await mediator.Execute(new AuthorizeRequest(action), cancellationToken).ConfigureAwait(false);
        return response.Success && response.Result.Access == AccessType.Allow;
    }

    /// <summary>
    /// Evaluates the authorization policies of <paramref name="action"/> for the current user without executing it, and
    /// returns the outcome with its explanation.
    /// </summary>
    /// <returns>
    /// The evaluated access and its reason. When the check itself fails, <see cref="AccessType.Unavailable"/> with the
    /// reason <c>Operation failed</c> - the error details are not passed on.
    /// </returns>
    public static async Task<AuthorizeRequestResponse> Authorize(this IMediator mediator, IMediatorAction action,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Execute(new AuthorizeRequest(action), cancellationToken).ConfigureAwait(false);
        return response.Success
            ? response.Result
            : new AuthorizeRequestResponse { Access = AccessType.Unavailable, Reason = "Operation failed" };
    }
}