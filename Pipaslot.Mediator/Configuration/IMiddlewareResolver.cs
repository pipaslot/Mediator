using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Configuration
{
    internal interface IMiddlewareResolver
    {
        public IEnumerable<Type> GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider);
    }
}
