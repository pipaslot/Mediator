using Pipaslot.Mediator.Abstractions;
using System;

namespace Pipaslot.Mediator.Middlewares.Pipelines;

internal readonly struct FuncPipelineCondition(Func<IMediatorAction, bool> function) : IPipelineCondition
{
    public bool Matches(IMediatorAction action)
    {
        return function(action);
    }
}