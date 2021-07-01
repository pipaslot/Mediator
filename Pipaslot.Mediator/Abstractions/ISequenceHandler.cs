namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>
    /// Provide handler prioritization in case of sequence execution.
    /// If combined with handler without this interface, then ISequenceHandler will be prioritized
    /// </summary>
    public interface ISequenceHandler
    {
        int Order { get; }
    }
}
