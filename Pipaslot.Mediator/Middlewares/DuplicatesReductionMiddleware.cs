using System;
using System.Collections.Generic;
using System.Threading;
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
        private readonly static Dictionary<Type, Dictionary<int, Task>> _running = new();
        private readonly object _lock = new();

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var type = action.GetType();
            var hashCode = action.GetHashCode();
            Task task;
            lock (_lock)
            {
                task = GetOrAddTask(type, hashCode, context, next);
            }

            try
            {
                await task;
            }
            finally
            {
                lock (_lock)
                {
                    Remove(type, hashCode);
                }
            }
        }

        private Task GetOrAddTask(Type actionType, int hashCode, MediatorContext context, MiddlewareDelegate next)
        {
            Task task;
            if (_running.TryGetValue(actionType, out var instances))
            {
                if (instances.TryGetValue(hashCode, out var runningTask))
                {
                    return runningTask;
                }
                task = next(context);
                instances.Add(hashCode, task);
                return task;
            }

            task = next(context);
            _running.Add(actionType, new Dictionary<int, Task> { { hashCode, task } });
            return task;
        }

        private void Remove(Type actionType, int hashCode)
        {
            if (_running.TryGetValue(actionType, out var instances))
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
