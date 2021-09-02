﻿namespace Pipaslot.Mediator
{
    /// <summary>
    /// Provide handler prioritization in case of sequence execution.
    /// If combined with handler without this interface, then ISequenceHandler will be prioritized because sequence/order value of unordered handlers will be int.MaxValue / 2
    /// </summary>
    public interface ISequenceHandler
    {
        int Order { get; }
    }
}
