namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>
    /// Action which returns data. All derived types can have own specific pipelines and handlers.
    /// </summary>
    /// <typeparam name="TResponse">Result data returned from handler execution</typeparam>
    public interface IRequest<out TResponse> : IRequest
    {
        
    }
    /// <summary>
    /// Marker interface for IRequest action type. 
    /// Use only for pipeline configuration to define middlewares applicable for this action type..
    /// </summary>
    public interface IRequest : IActionMarker
    {

    }
}