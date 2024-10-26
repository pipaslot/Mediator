using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Configuration;

internal class MiddlewareDefinition : IMiddlewareResolver
{
    public Type Type { get; }
    public object[]? Parameters { get; }

    public MiddlewareDefinition(Type middlewareType, object[]? parameters = null)
    {
        Type = middlewareType;
        Parameters = parameters;
    }

    public IEnumerable<MiddlewareDefinition> GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider)
    {
        yield return this;
    }
}