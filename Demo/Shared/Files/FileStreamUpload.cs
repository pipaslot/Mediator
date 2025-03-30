namespace Demo.Shared.Files;

/// <summary>
/// Upload file selected by the user to the server. The content is not serialized
/// </summary>
/// <param name="File"></param>
[AnonymousPolicy]
public record FileStreamUpload(FileStreamDto File) : IMediatorAction;