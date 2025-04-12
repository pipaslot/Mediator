using Pipaslot.Mediator.Abstractions;
using System;

namespace Pipaslot.Mediator.Configuration;

internal interface IMiddlewareResolver
{
    internal MiddlewareDefinition[] GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider);
}