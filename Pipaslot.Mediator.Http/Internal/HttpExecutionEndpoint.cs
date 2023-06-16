using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Http.Configuration;

namespace Pipaslot.Mediator.Http.Internal
{
    internal enum HttpExecutionEndpoint

    {
        /// <summary>
        /// There is not HTTP request arround the Mediator execution. It was executed internally as background service
        /// </summary>
        NoEndpoint = 0,
        /// <summary>
        /// The Mediator was started via HTTP, but only through its own dedicated endpoint.
        /// </summary>
        MediatorEndpoint,
        /// <summary>
        /// The mediator was started via HTTP through unknow Endpoint
        /// </summary>
        CustomEndpoint
    }
}
