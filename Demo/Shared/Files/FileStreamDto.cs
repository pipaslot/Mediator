namespace Demo.Shared.Files;

/// <summary>
/// Object using streaming instead of JSON serialization. Streaming us more effective way how to push large files from client to server.
/// </summary>
public class FileStreamDto(Stream content, string contentType, string name)
{
    /// <summary>File name</summary>
    public string Name { get; set; } = name;

    /// <summary>File content</summary>
    public Stream Content { get; set; } = content;

    /// <summary>Content type</summary>
    public string ContentType { get; set; } = contentType;
}