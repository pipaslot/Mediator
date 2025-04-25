using Pipaslot.Mediator.Abstractions;
using System;

namespace Pipaslot.Mediator.Middlewares.Pipelines;

internal readonly struct TypeBasedPipelineCondition(Type[] types) : IPipelineCondition
{
    public bool Matches(IMediatorAction action)
    {
        var actionType = action.GetType();
        foreach (var t in types)
        {
            if (t.IsAssignableFrom(actionType))
                return true;
        }
        return false;
    }
}