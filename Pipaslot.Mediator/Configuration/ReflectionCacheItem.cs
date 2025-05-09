using System;

namespace Pipaslot.Mediator.Configuration;

internal record ReflectionCacheItem(Type ExecutorType, Type? ResultType)
{
    public bool HasResultType => ResultType != null;
}