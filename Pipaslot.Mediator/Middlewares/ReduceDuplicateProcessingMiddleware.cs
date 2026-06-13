using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Reduce action processing to only one at the same time for the same action type with the same properties.
/// This is useful when you know that your application executes the same action multiple times but you want to reduce the server load. 
/// IMPORTANT!: object method GetHashcode() is used for evaluating object similarities.
/// WARNING!: If you manipulate with the context through MediatorContextAccess, then you will modify the original context, not his copy!
/// </summary>
public class ReduceDuplicateProcessingMiddleware : IMediatorMiddleware
{
    private readonly Dictionary<Type, Dictionary<int, RunningTask>> _running = new();
    private readonly object _lock = new();

    private sealed record RunningTask(Task<MediatorContext> Task, CancellationToken CancellationToken)
    {
        public bool CanReuse()
        {
            return !Task.IsCompleted && !CancellationToken.IsCancellationRequested;
        }
    }

    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        var type = context.Action.GetType();
        var hashCode = context.Action.GetHashCode();
        RunningTask runningTask;
        lock (_lock)
        {
            runningTask = GetOrAddTask(type, hashCode, context, next);
        }

        try
        {
            var innerContext = await runningTask.Task.ConfigureAwait(false);
            context.Append(innerContext);
            context.Status = innerContext.Status;
        }
        finally
        {
            lock (_lock)
            {
                Remove(type, hashCode, runningTask.Task);
            }
        }
    }

    private RunningTask GetOrAddTask(Type actionType, int hashCode, MediatorContext context, MiddlewareDelegate next)
    {
        if (_running.TryGetValue(actionType, out var instances) && instances != null)
        {
            if (instances.TryGetValue(hashCode, out var runningTask) && runningTask != null)
            {
                if (runningTask.CanReuse())
                {
                    return runningTask;
                }

                instances.Remove(hashCode);
            }

            var created = CreateRunningTask(context, next);
            instances.Add(hashCode, created);
            return created;
        }

        var createdNewType = CreateRunningTask(context, next);
        _running.Add(actionType, new Dictionary<int, RunningTask> { { hashCode, createdNewType } });
        return createdNewType;
    }

    private static RunningTask CreateRunningTask(MediatorContext context, MiddlewareDelegate next)
    {
        var contextCopy = context.CopyEmpty();
        var task = Run(contextCopy, next);
        return new RunningTask(task, contextCopy.CancellationToken);
    }

    private static async Task<MediatorContext> Run(MediatorContext context, MiddlewareDelegate next)
    {
        await next(context).ConfigureAwait(false);
        return context;
    }

    private void Remove(Type actionType, int hashCode, Task<MediatorContext> ownerTask)
    {
        if (_running.TryGetValue(actionType, out var instances) && instances != null)
        {
            if (instances.TryGetValue(hashCode, out var current) && ReferenceEquals(current.Task, ownerTask))
            {
                instances.Remove(hashCode);
            }

            if (instances.Count == 0)
            {
                _running.Remove(actionType);
            }
        }
    }
}