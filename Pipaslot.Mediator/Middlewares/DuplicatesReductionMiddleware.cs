using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Reduce action processing to only one at the same time for the same action type with the same properties.
    /// This is useful when you know that your application executes the same action multiple times but you want to reduce the server load. 
    /// IMPORTANT!: object method GetHashcode() is used for evaluating object similarities
    /// </summary>
    public class ReduceDuplicateProcessingMiddleware : IMediatorMiddleware
    {
        private readonly static Dictionary<Type, Dictionary<int, Task<MediatorContext>>> _running = new();
        private readonly object _lock = new();

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var type = context.Action.GetType();
            var hashCode = context.Action.GetHashCode();
            Task<MediatorContext> task;
            lock (_lock)
            {
                task = GetOrAddTask(type, hashCode, context, next);
            }

            try
            {
                var innerContext = await task.ConfigureAwait(false);
                context.Append(innerContext);
                context.Status = innerContext.Status;
            }
            finally
            {
                lock (_lock)
                {
                    Remove(type, hashCode);
                }
            }
        }

        private static Task<MediatorContext> GetOrAddTask(Type actionType, int hashCode, MediatorContext context, MiddlewareDelegate next)
        {
            var contextCopy = context.CopyEmpty();
            Task<MediatorContext> task;
            if (_running.TryGetValue(actionType, out var instances) && instances != null)
            {
                if (instances.TryGetValue(hashCode, out var runningTask) && runningTask != null)
                {
                    return runningTask;
                }
                task = Run(contextCopy, next);
                instances.Add(hashCode, task);
                return task;
            }

            task = Run(contextCopy, next);
            _running.Add(actionType, new Dictionary<int, Task<MediatorContext>> { { hashCode, task } });
            return task;
        }

        private static async Task<MediatorContext> Run(MediatorContext context, MiddlewareDelegate next)
        {
            await next(context).ConfigureAwait(false);
            return context;
        }

        private static void Remove(Type actionType, int hashCode)
        {
            if (_running.TryGetValue(actionType, out var instances) && instances != null)
            {
                instances.Remove(hashCode);
                if (instances.Count == 0)
                {
                    _running.Remove(actionType);
                }
            }
        }
    }
}
