using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests.Fakes;

internal class FakePostRequest : HttpRequest
{
    public FakePostRequest(string action)
    {
        Body = new MemoryStream(Encoding.UTF8.GetBytes(action));
        Query = new Mock<IQueryCollection>().Object;
    }

    public override HttpContext HttpContext => throw new NotImplementedException();

    public override string Method { get; set; } = "POST";
    public override string Scheme { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override bool IsHttps { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override HostString Host { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override PathString PathBase { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override PathString Path { get; set; } = MediatorConstants.Endpoint;
    public override QueryString QueryString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override IQueryCollection Query { get; set; }
    public override string Protocol { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override IHeaderDictionary Headers => throw new NotImplementedException();

    public override IRequestCookieCollection Cookies
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override long? ContentLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override string ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override Stream Body { get; set; }

    public override bool HasFormContentType => throw new NotImplementedException();

    public override IFormCollection Form { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}