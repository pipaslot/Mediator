using System;

namespace Pipaslot.Mediator.Configuration;

/// <summary>
/// Routing entry pointing a registered exception type to its closed-generic executor.
/// </summary>
internal sealed class ExceptionHandlerEntry(Type exceptionType, Type executorType)
{
    public Type ExceptionType { get; } = exceptionType;
    public Type ExecutorType { get; } = executorType;
}
