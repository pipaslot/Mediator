using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground;

public class DemoDownloadHandler : IMessageHandler<DemoDownload>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DemoDownloadHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(DemoDownload action, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException($"Action {nameof(DemoDownload)} can be used only for request over HTTP.");
        }

        httpContext.Response.ContentType = "text/plain";
        httpContext.Response.Headers["Content-Disposition"] = $"attachment;filename={action.FileName}.txt";
        await httpContext.Response.WriteAsync("Hello File!", cancellationToken);
    }
}