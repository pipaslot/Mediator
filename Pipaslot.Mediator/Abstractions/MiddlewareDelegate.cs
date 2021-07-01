using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Abstractions
{

    /// <summary>
    /// Represents an async continuation for the next task to execute in the pipeline
    /// </summary>
    /// <returns>Awaitable task</returns>
    public delegate Task MiddlewareDelegate();


    /// <summary>
    /// Represents an async continuation for the next task to execute in the pipeline
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <returns>Awaitable task returning a <typeparamref name="TResponse" /></returns>
    public delegate Task<TResponse> MiddlewareDelegate<TResponse>();
}
