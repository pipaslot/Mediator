using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares.Handlers;

/// <summary>
/// Abstraction for generic handler executors to avoid reflection
/// </summary>
internal abstract class HandlerExecutor
{
    public abstract Task Execute(MediatorContext context);

    internal abstract object[] GetHandlers(IServiceProvider services);
    
    internal static bool ValidateHandlers<T>(T[] handlers, Type actionType)
    {
        if (!handlers.Any())
        {
            return false;
        }

        var anyIsSequence = false;
        var anyIsConcurrent = false;
        var anyIsSingle = false;
        foreach (var handler in handlers)
        {
            var isSequence = handler is ISequenceHandler;
            var isConcurrent = handler is IConcurrentHandler;
            var isSingle = !isSequence && !isConcurrent;
            anyIsSequence = anyIsSequence || isSequence;
            anyIsConcurrent = anyIsConcurrent || isConcurrent;
            anyIsSingle = anyIsSingle || isSingle;
        }

        if ((anyIsConcurrent && anyIsSequence)
            || (anyIsConcurrent && anyIsSingle)
            || (anyIsSequence && anyIsSingle))
        {
            throw MediatorException.CreateForCanNotCombineHandlers(actionType, handlers);
        }

        if (anyIsSingle && handlers.Length > 1)
        {
            throw MediatorException.CreateForDuplicateHandlers(actionType, handlers);
        }

        return anyIsConcurrent;
    }

    protected static T[] Sort<T>(T[] handlers)
    {
        return handlers
            .Select(h => new { Handler = h, Order = h is ISequenceHandler s ? s.Order : int.MaxValue / 2 })
            .OrderBy(i => i.Order)
            .Select(i => i.Handler)
            .ToArray();
    }
}