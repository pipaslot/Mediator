using System.IO;

namespace Pipaslot.Mediator.Http.Serialization;

/// <summary>
/// Stream extracted from the contract during serialization
/// </summary>
/// <param name="id">Identifier that links the stream to its place in the serialized action; it is the file name of the multipart part.</param>
/// <param name="stream">Content of the stream.</param>
public readonly struct StreamContract(string id, Stream stream)
{
    /// <summary>
    /// Identifier that links the stream to its place in the serialized action; it is the file name of the multipart part.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Content of the stream. The consumer of the contract is responsible for disposing it.
    /// </summary>
    public Stream Stream { get; } = stream;
}