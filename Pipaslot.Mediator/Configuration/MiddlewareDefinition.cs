using Pipaslot.Mediator.Abstractions;
using System;

namespace Pipaslot.Mediator.Configuration;

internal readonly record struct MiddlewareDefinition(Type Type, object[]? Parameters = null) : IMiddlewareResolver
{
    public MiddlewareDefinition[] GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider)
    {
        return [this];
    }
}