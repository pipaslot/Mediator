using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Track actions processed by middleware through exposed events
/// </summary>
public class ActionEventsMiddleware : IMediatorMiddleware
{
    private readonly ConcurrentDictionary<Guid, IMediatorAction> _runningActions = new();

    /// <summary>
    /// Event fired when every Action is getting executed or dispatched. Allows trackin all actions executed via mediator.
    /// </summary>
    public event EventHandler<ActionStartedEventArgs>? ActionStarted;

    /// <summary>
    /// Event fired when any action started. When multiple actions starts together, then only single event is triggered. 
    /// Usefull for showing global spinner.
    /// </summary>
    public event EventHandler? ProcessingStarted;

    /// <summary>
    /// Event fired after every action completion. Allows trackin all actions executed via mediator.
    /// </summary>
    public event EventHandler<ActionCompletedEventArgs>? ActionCompleted;

    /// <summary>
    /// Event fired once all actions are completed. 
    /// Usefull for hiding global spinner.
    /// </summary>
    public event EventHandler? ProcessingCompleted;

    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        AddToQueue(context);
        try
        {
            await next(context).ConfigureAwait(false);
        }
        finally
        {
            RemoveFromQueue(context);
        }
    }

    private void AddToQueue(MediatorContext context)
    {
        _runningActions.TryAdd(context.Guid, context.Action);
        var runningActions = _runningActions.Values.ToArray();
        ActionStarted?.Invoke(this, new ActionStartedEventArgs(context.Action, runningActions));
        if (ProcessingStarted != null && runningActions.Length == 1)
        {
            ProcessingStarted.Invoke(this, new EventArgs());
        }
    }

    private void RemoveFromQueue(MediatorContext context)
    {
        _runningActions.TryRemove(context.Guid, out var action);
        var runningActions = _runningActions.Values.ToArray();
        if (action != null)
        {
            ActionCompleted?.Invoke(this, new ActionCompletedEventArgs(action, runningActions));
        }

        if (ProcessingCompleted != null && runningActions.Length == 0)
        {
            ProcessingCompleted.Invoke(this, new EventArgs());
        }
    }
}