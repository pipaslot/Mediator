using System.IO;

namespace Pipaslot.Mediator.Http.Serialization;

/// <summary>
/// Stream extracted from the contract during serialization
/// </summary>
/// <param name="id"></param>
/// <param name="stream"></param>
public readonly struct StreamContract(string id, Stream stream)
{
    public string Id { get; } = id;
    public Stream Stream { get; } = stream;
}