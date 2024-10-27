using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Configuration;

internal class MiddlewareDefinition(Type middlewareType, object[]? parameters = null) : IMiddlewareResolver
{
    public Type Type { get; } = middlewareType;
    public object[]? Parameters { get; } = parameters;

    public IEnumerable<MiddlewareDefinition> GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider)
    {
        yield return this;
    }
}