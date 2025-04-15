using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Configuration;

internal interface IMiddlewareResolver
{
    internal void CollectMiddlewares(IMediatorAction action, IServiceProvider serviceProvider, List<Mediator.MiddlewarePair> collection);
}