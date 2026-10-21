using System.Collections.Generic;

namespace Pipaslot.Mediator.Http.Serialization;

/// <summary>
/// An action serialized by <see cref="IContractSerializer"/> for sending to the server.
/// </summary>
/// <param name="json">JSON representation of the action.</param>
/// <param name="streams">Streams extracted from the action.</param>
public readonly struct SerializedRequest(string json, ICollection<StreamContract> streams)
{
    /// <summary>
    /// The action serialized by <see cref="IContractSerializer"/> as the wire contract between the client and the server; stream
    /// properties are not part of it but travel separately in <see cref="Streams"/>.
    /// </summary>
    public string Json { get; } = json;

    /// <summary>
    /// Streams pulled out of the action. When any is present, the client sends the request as multipart form data instead
    /// of plain JSON to avoid encoding binary content into the JSON.
    /// </summary>
    public ICollection<StreamContract> Streams { get; } = streams;
}