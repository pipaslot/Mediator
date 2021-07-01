namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>
    /// Action which does not return data. All derived types can have own specific pipelines and handlers.
    /// </summary>
    public interface IMessage : IActionMarker
    {
    }
}
