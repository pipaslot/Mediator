using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Http;

namespace Demo.Shared.Playground;

[AnonymousPolicy]
public record DemoDownload(string FileName) : IRequest<DemoDownloadResult>;

public record DemoDownloadResult(string FileName, string Content) : IMediatorHttpResult
{
    public Task ApplyAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "text/plain";
        httpContext.Response.Headers["Content-Disposition"] = $"attachment;filename={FileName}.txt";
        return httpContext.Response.WriteAsync(Content, cancellationToken);
    }
}