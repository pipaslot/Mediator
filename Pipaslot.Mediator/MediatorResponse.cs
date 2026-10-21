using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Notifications;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator;

/// <summary>
/// Default <see cref="IMediatorResponse{TResult}"/> implementation, returned by <see cref="IMediator.Execute{TResult}"/>.
/// </summary>
/// <remarks>
/// Constructing one is for code that replaces the mediator's own execution - a custom
/// <see cref="Middlewares.IExecutionMiddleware"/>, a transport, or a test double of <see cref="IMediator"/>.
/// </remarks>
public class MediatorResponse<TResult> : MediatorResponse, IMediatorResponse<TResult>
{
    /// <summary>
    /// Creates a failed response whose only result is an <see cref="NotificationType.Error"/> notification with
    /// <paramref name="errorMessage"/> and an empty source.
    /// </summary>
    public MediatorResponse(string errorMessage) : base(errorMessage)
    {
    }

    /// <summary>
    /// Creates a failed response whose only result is an <see cref="NotificationType.Error"/> notification with
    /// <paramref name="errorMessage"/>, attributed to <paramref name="action"/> through the notification's source.
    /// </summary>
    public MediatorResponse(string errorMessage, IMediatorAction action) : base(errorMessage, action)
    {
    }

    /// <summary>
    /// Creates a response with the given outcome. <paramref name="results"/> is copied, and is not checked against
    /// <paramref name="success"/> - include the result of type <typeparamref name="TResult"/>, otherwise
    /// <see cref="IMediatorResponse{TResult}.Result"/> has nothing to return.
    /// </summary>
    public MediatorResponse(bool success, IEnumerable<object> results) : base(success, results)
    {
    }

    TResult IMediatorResponse<TResult>.Result => this.GetResult<TResult>();
}

/// <summary>
/// Default <see cref="IMediatorResponse"/> implementation, returned by <see cref="IMediator.Dispatch"/>.
/// </summary>
/// <remarks>
/// Constructing one is for code that replaces the mediator's own execution - a custom
/// <see cref="Middlewares.IExecutionMiddleware"/>, a transport, or a test double of <see cref="IMediator"/>. Use
/// <see cref="MediatorResponse{TResult}"/> where an <see cref="IMediatorResponse{TResult}"/> is expected.
/// </remarks>
public class MediatorResponse : IMediatorResponse
{
    /// <summary>
    /// Creates a failed response whose only result is an <see cref="NotificationType.Error"/> notification with
    /// <paramref name="errorMessage"/> and an empty source.
    /// </summary>
    public MediatorResponse(string errorMessage)
    {
        Success = false;
        Results = [Notification.Error(errorMessage)];
    }

    /// <summary>
    /// Creates a failed response whose only result is an <see cref="NotificationType.Error"/> notification with
    /// <paramref name="errorMessage"/>, attributed to <paramref name="action"/> through the notification's source.
    /// </summary>
    public MediatorResponse(string errorMessage, IMediatorAction action)
    {
        Success = false;
        Results = [Notification.Error(errorMessage, action)];
    }

    /// <summary>
    /// Creates a response with the given outcome. <paramref name="results"/> is copied, and is not checked against
    /// <paramref name="success"/> - a failed response without an error notification carries no error message.
    /// </summary>
    public MediatorResponse(bool success, IEnumerable<object> results)
    {
        Success = success;
        Results = results.ToArray();
    }

    /// <inheritdoc/>
    public bool Success { get; }

    /// <inheritdoc/>
    public bool Failure => !Success;

    /// <inheritdoc/>
    public object[] Results { get; }
}