using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Configuration;

internal readonly record struct MiddlewareDefinition(Type Type, object[]? Parameters = null) : IMiddlewareResolver
{
    public void CollectMiddlewares(IMediatorAction action, IServiceProvider serviceProvider, List<Mediator.MiddlewarePair> collection)
    {
        collection.Add(new Mediator.MiddlewarePair(null, Type, Parameters));
    }
}