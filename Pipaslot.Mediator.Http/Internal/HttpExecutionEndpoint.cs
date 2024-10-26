namespace Pipaslot.Mediator.Http.Internal;

internal enum HttpExecutionEndpoint

{
    /// <summary>
    /// There is no HTTP request arround the Mediator execution. It was run internally as a background service.
    /// </summary>
    NoEndpoint = 0,

    /// <summary>
    /// The Mediator was started via HTTP, but only through its own dedicated endpoint.
    /// </summary>
    MediatorEndpoint = 1,

    /// <summary>
    /// The mediator was started via HTTP through unknow/custom Endpoint. 
    /// </summary>
    CustomEndpoint = 2
}