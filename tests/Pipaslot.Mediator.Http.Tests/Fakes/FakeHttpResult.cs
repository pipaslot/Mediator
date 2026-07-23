using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests.Fakes;

internal class FakeHttpResult(Action<HttpContext>? onApply = null) : IMediatorHttpResult
{
    public bool Applied { get; private set; }

    public Task ApplyAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        Applied = true;
        onApply?.Invoke(httpContext);
        return Task.CompletedTask;
    }
}
