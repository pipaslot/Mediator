using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Configuration
{
    internal interface IMiddlewareResolver
    {
        internal IEnumerable<MiddlewareDefinition> GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider);
    }
}
