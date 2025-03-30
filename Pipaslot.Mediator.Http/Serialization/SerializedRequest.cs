using System.Collections.Generic;

namespace Pipaslot.Mediator.Http.Serialization;

public readonly struct SerializedRequest(string json, ICollection<StreamContract> streams)
{
    public string Json { get; } = json;
    public ICollection<StreamContract> Streams { get; } = streams;
}