using Demo.Shared.Playground;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http;

namespace Demo.Server.Handlers.Playground;

public class DemoDownloadHandler : IMediatorHandler<DemoDownload, IMediatorHttpResult>
{
    public Task<IMediatorHttpResult> Handle(DemoDownload action, CancellationToken cancellationToken)
    {
        return Task.FromResult<IMediatorHttpResult>(new DemoDownloadResult(action.FileName, "Hello File!"));
    }
}

internal class DemoDownloadResult(string fileName, string content) : IMediatorHttpResult
{
    public Task ApplyAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "text/plain";
        httpContext.Response.Headers["Content-Disposition"] = $"attachment;filename={fileName}.txt";
        return httpContext.Response.WriteAsync(content, cancellationToken);
    }
}