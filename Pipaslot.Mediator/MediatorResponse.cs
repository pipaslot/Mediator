using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Notifications;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator;

public class MediatorResponse<TResult> : MediatorResponse, IMediatorResponse<TResult>
{
    public MediatorResponse(string errorMessage) : base(errorMessage)
    {
    }

    public MediatorResponse(string errorMessage, IMediatorAction action) : base(errorMessage, action)
    {
    }

    public MediatorResponse(bool success, IEnumerable<object> results) : base(success, results)
    {
    }

    TResult IMediatorResponse<TResult>.Result => this.GetResult<TResult>();
}

public class MediatorResponse : IMediatorResponse
{
    public MediatorResponse(string errorMessage)
    {
        Success = false;
        Results = [Notification.Error(errorMessage)];
    }

    public MediatorResponse(string errorMessage, IMediatorAction action)
    {
        Success = false;
        Results = [Notification.Error(errorMessage, action)];
    }

    public MediatorResponse(bool success, IEnumerable<object> results)
    {
        Success = success;
        Results = results.ToArray();
    }

    public bool Success { get; }
    public bool Failure => !Success;
    public object[] Results { get; }
}