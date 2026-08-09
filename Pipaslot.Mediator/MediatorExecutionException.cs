using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator;

/// <summary>
/// Exception related to mediator execution, middleware processing and unexpected status during execution.
/// </summary>
public class MediatorExecutionException : MediatorException
{
    /// <summary>
    /// Response containing all information gathered from Mediator execution
    /// </summary>
    public IMediatorResponse Response { get; }

    public MediatorExecutionException(string message, MediatorContext? context) : base(message)
    {
        Response = new MediatorResponse(false, context?.Results ?? Array.Empty<object>());
    }

    public MediatorExecutionException(string message, IMediatorResponse response) : base($"{message} Errors: ['{GetErrors(response.Results)}']")
    {
        Response = response;
    }

    public MediatorExecutionException(IMediatorResponse response) : base(GetErrors(response.Results))
    {
        Response = response;
    }

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