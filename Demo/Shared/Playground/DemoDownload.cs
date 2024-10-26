using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared.Playground;

[AnonymousPolicy]
public class DemoDownload : IMessage
{
    public string FileName { get; init; }

    public DemoDownload(string fileName)
    {
        FileName = fileName;
    }
}