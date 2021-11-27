﻿namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Marks middleare as final/last wchich executes handlers. Pipeline ends with this milleware evenf it some next middlewares are registered.
    /// This interface was introduced to connect pipeline definitions and query handler existence check.
    /// </summary>
    public interface IExecutionMiddleware : IMediatorMiddleware
    {
        bool ExecuteMultipleHandlers { get; }
    }
}
