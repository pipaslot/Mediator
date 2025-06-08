using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests.Fakes;

internal class FakeResponse(bool hasStarted = false) : HttpResponse
{
    public override HttpContext HttpContext => throw new NotImplementedException();

    public override int StatusCode { get; set; } = 200;

    public override IHeaderDictionary Headers => throw new NotImplementedException();

    public override Stream Body { get; set; } = new Mock<Stream>().Object;
    public override long? ContentLength { get; set; }
    public override string ContentType { get; set; } = string.Empty;

    public override IResponseCookies Cookies => throw new NotImplementedException();

    public override bool HasStarted => hasStarted;

    public override void OnCompleted(Func<object, Task> callback, object state)
    {
        throw new NotImplementedException();
    }

    public override void OnStarting(Func<object, Task> callback, object state)
    {
        throw new NotImplementedException();
    }

    public override void Redirect(string location, bool permanent)
    {
        throw new NotImplementedException();
    }
}