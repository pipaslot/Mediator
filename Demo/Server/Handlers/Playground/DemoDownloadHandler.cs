using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground;

public class DemoDownloadHandler(IHttpContextAccessor httpContextAccessor) : IMessageHandler<DemoDownload>
{
    public async Task Handle(DemoDownload action, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException($"Action {nameof(DemoDownload)} can be used only for request over HTTP.");
        }

        httpContext.Response.ContentType = "text/plain";
        httpContext.Response.Headers["Content-Disposition"] = $"attachment;filename={action.FileName}.txt";
        await httpContext.Response.WriteAsync("Hello File!", cancellationToken);
    }
}