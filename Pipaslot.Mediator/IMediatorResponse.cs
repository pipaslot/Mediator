using Pipaslot.Mediator.Notifications;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator;

/// <summary>
/// Outcome of <see cref="IMediator.Execute{TResult}"/>: the success flag and collected results of
/// <see cref="IMediatorResponse"/>, plus the handler's result picked out of them.
/// </summary>
/// <remarks>
/// A wrapper, not the bare result MediatR's <c>Send</c> returns - check <see cref="IMediatorResponse.Success"/> before
/// reading <see cref="Result"/>. Use <see cref="IMediator.ExecuteUnhandled{TResult}"/> to get the bare value and an
/// exception on failure instead.
/// </remarks>
public interface IMediatorResponse<TResult> : IMediatorResponse
{
    /// <summary>
    /// The first entry of <see cref="IMediatorResponse.Results"/> of type <typeparamref name="TResult"/>. Meaningful only
    /// when <see cref="IMediatorResponse.Success"/> is true; a handler that returned null yields null here. When
    /// <see cref="IMediatorResponse.Results"/> holds no value of that type (typically after a failure) it is
    /// <c>default(TResult)</c> - null for a reference type, for example 0 for <c>int</c> - and reading it does not throw.
    /// </summary>
    TResult Result { get; }
}

/// <summary>
/// Outcome of <see cref="IMediator.Dispatch"/>, and the base of <see cref="IMediatorResponse{TResult}"/>: whether the
/// action succeeded, and everything that middlewares and handlers reported along the way.
/// </summary>
/// <remarks>
/// A failure is reported here, not thrown - a caller that ignores the response ignores the failure too. Read the error
/// messages with <see cref="MediatorResponseExtensions.GetErrorMessage"/>, or call <see cref="IMediator.DispatchUnhandled"/>
/// to get an exception instead. Over the HTTP transport this is the object that is serialized to the client.
/// </remarks>
public interface IMediatorResponse
{
    /// <summary>
    /// True when the pipeline finished with <see cref="Middlewares.ExecutionStatus.Succeeded"/>. False after a failure
    /// reported by a middleware or handler, after an exception caught by the mediator, and when no handler was found.
    /// </summary>
    /// <remarks>
    /// Notifications do not affect it: a response can succeed and still carry
    /// <see cref="Notifications.NotificationType.Error"/> notifications added via <see cref="IMediatorFacade.AddNotification"/>.
    /// </remarks>
    bool Success { get; }

    /// <summary>
    /// Negated value of Success 
    /// </summary>
    bool Failure { get; }

    /// <summary>
    /// Results provided by middlewares and handlers - the handler's result as well as every
    /// <see cref="Notification"/>, including the error messages of a failed action and those propagated from nested calls.
    /// </summary>
    object[] Results { get; }
}

/// <summary>
/// Reads notifications and error messages out of <see cref="IMediatorResponse.Results"/>.
/// </summary>
public static class MediatorResponseExtensions
{
    internal static bool HasResult<TResult>(this IMediatorResponse response)
    {
        return response.Results.Any(r => r is NullActionResult or TResult);
    }

    internal static TResult GetResult<TResult>(this IMediatorResponse response)
    {
        return response.Results.FirstOrDefault(r => r is TResult) is TResult result ? result : default!;
    }

    internal static IEnumerable<Notification> GetNotifications(this ICollection<object> results)
    {
        return results
            .Where(r => r is Notification)
            .Cast<Notification>();
    }

    internal static IEnumerable<string> GetErrorMessages(this IEnumerable<Notification> notifications)
    {
        return notifications
            .Where(n => n.Type.IsError())
            .Select(n => n.Content);
    }

    /// <summary>
    /// Joins <paramref name="errors"/> with <c>;</c> and no space - the separator <see cref="GetErrorMessage"/> uses.
    /// </summary>
    public static string JoinErrorMessages(this IEnumerable<string> errors)
    {
        return string.Join(";", errors);
    }

    /// <summary>
    /// All notifications in <see cref="IMediatorResponse.Results"/>, of every <see cref="NotificationType"/>.
    /// </summary>
    public static IEnumerable<Notification> GetNotifications(this IMediatorResponse response)
    {
        return response.Results
            .GetNotifications();
    }

    /// <summary>
    /// Contents of the <see cref="NotificationType.Error"/> notifications in <see cref="IMediatorResponse.Results"/>.
    /// </summary>
    /// <remarks>
    /// For an exception the mediator caught, this is the message produced by the registered
    /// <see cref="IMediatorExceptionHandler{TException}"/>, or the generic one. The exception's own message appears only
    /// when a handler passes it on, as <see cref="LegacyExceptionMessageHandler"/> does. On the HTTP client, a transport
    /// failure (unreachable server, unparsable response) is reported with the message of the transport exception.
    /// </remarks>
    public static IEnumerable<string> GetErrorMessages(this IMediatorResponse response)
    {
        return response.Results
            .GetNotifications()
            .GetErrorMessages();
    }

    /// <summary>
    /// <see cref="GetErrorMessages(IMediatorResponse)"/> joined into one string by <see cref="JoinErrorMessages"/>; empty when there are none.
    /// </summary>
    public static string GetErrorMessage(this IMediatorResponse response)
    {
        return response.Results
            .GetNotifications()
            .GetErrorMessages()
            .JoinErrorMessages();
    }
}