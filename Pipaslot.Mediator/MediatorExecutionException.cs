using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator;

/// <summary>
/// Exception related to mediator execution, middleware processing and unexpected status during execution.
/// </summary>
/// <remarks>
/// The common base of <see cref="MediatorNoHandlerFoundException"/>, <see cref="MediatorMissingResultException"/> and
/// <see cref="MediatorUnhandledErrorException"/>; catch it to get at <see cref="Response"/> regardless of which one was
/// thrown. Only the <c>*Unhandled</c> entry points of <see cref="IMediator"/> let it reach the caller -
/// <see cref="IMediator.Dispatch"/>/<see cref="IMediator.Execute{TResult}"/> report the failure in the response instead.
/// </remarks>
public class MediatorExecutionException : MediatorException
{
    /// <summary>
    /// Response containing all information gathered from Mediator execution
    /// </summary>
    public IMediatorResponse Response { get; }

    /// <summary>
    /// Creates the exception with a failed <see cref="Response"/> holding the results collected in
    /// <paramref name="context"/> so far, or no results when <paramref name="context"/> is null.
    /// </summary>
    public MediatorExecutionException(string message, MediatorContext? context) : base(message)
    {
        Response = new MediatorResponse(false, context?.Results ?? Array.Empty<object>());
    }

    /// <summary>
    /// Creates the exception for an existing <paramref name="response"/>; the error messages it carries are appended to
    /// <paramref name="message"/>.
    /// </summary>
    public MediatorExecutionException(string message, IMediatorResponse response) : base($"{message} Errors: ['{GetErrors(response.Results)}']")
    {
        Response = response;
    }

    /// <summary>
    /// Creates the exception for an existing <paramref name="response"/>, with its error messages as the exception message.
    /// </summary>
    public MediatorExecutionException(IMediatorResponse response) : base(GetErrors(response.Results))
    {
        Response = response;
    }

    /// <summary>
    /// Contents of the <see cref="NotificationType.Error"/> notifications in <paramref name="results"/>, joined by
    /// <c>"; "</c>.
    /// </summary>
    protected static string GetErrors(IReadOnlyCollection<object> results)
    {
        var errors = results
            .Where(r => r is Notification)
            .Cast<Notification>()
            .Where(n => n.Type.IsError())
            .Select(n => n.Content);
        return string.Join("; ", errors);
    }
}