using Demo.Shared.Playground;
using Pipaslot.Mediator.Abstractions;

namespace Demo.Server.Handlers.Playground;

public class DemoDownloadHandler : IMediatorHandler<DemoDownload, DemoDownloadResult>
{
    public Task<DemoDownloadResult> Handle(DemoDownload action, CancellationToken cancellationToken)
    {
        return Task.FromResult(new DemoDownloadResult(action.FileName, "Hello File!"));
    }
}