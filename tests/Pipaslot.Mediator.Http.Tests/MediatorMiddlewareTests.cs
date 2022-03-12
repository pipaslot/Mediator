using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests
{
    public class MediatorMiddlewareTests
    {
        [Fact]
        public async Task PostRequestWillBeProapgatedToMediator()
        {
            await Execute(new PostRequest());
        }

        [Fact]
        public async Task GetRequestWillBeProapgatedToMediator()
        {
            await Execute(new GetRequest());
        }

        private async Task Execute(HttpRequest request)
        {
            var mediatorMock = new Mock<IMediator>();
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddMediatorServer();
            collection.AddScoped<MediatorMiddleware>();
            collection.AddScoped<RequestDelegate>(s => (c) => Task.CompletedTask);
            collection.AddSingleton<IMediator>(mediatorMock.Object);
            var services = collection.BuildServiceProvider();
            var sut = services.GetRequiredService<MediatorMiddleware>();

            var context = new FakeContext(request);
            context.RequestServices = services;
            await sut.Invoke(context);

            mediatorMock.Verify(m => m.Dispatch(It.IsAny<NopMessage>(), It.IsAny<CancellationToken>()));
        }

        private class FakeContext : HttpContext
        {
            private HttpRequest _request;

            public FakeContext(HttpRequest request)
            {
                _request = request;
            }

            public override IFeatureCollection Features => throw new NotImplementedException();

            public override HttpRequest Request => _request;

            public override HttpResponse Response => new FakeResponse();

            public override ConnectionInfo Connection => throw new NotImplementedException();

            public override WebSocketManager WebSockets => throw new NotImplementedException();

            public override AuthenticationManager Authentication => throw new NotImplementedException();

            public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override IServiceProvider RequestServices { get; set; }
            public override CancellationToken RequestAborted { get; set; } = CancellationToken.None;
            public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override void Abort()
            {
            }
        }

        private class PostRequest : HttpRequest
        {
            public PostRequest()
            {
                var body = "{\"Content\":{},\"Type\":\"Pipaslot.Mediator.Tests.ValidActions.NopMessage, Pipaslot.Mediator.Tests.ValidActions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}";
                Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
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

            public override IRequestCookieCollection Cookies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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
        private class GetRequest : PostRequest
        {
            public override string Method { get; set; } = "GET";
            public override IQueryCollection Query { get; set; } = new QueryCollection();
            public override Stream Body { get; set; } = null;

            private class QueryCollection : IQueryCollection
            {

            }
        }
        private class FakeResponse : HttpResponse
        {
            public override HttpContext HttpContext => throw new NotImplementedException();

            public override int StatusCode { get; set; } = 200;

            public override IHeaderDictionary Headers => throw new NotImplementedException();

            public override Stream Body { get; set; }
            public override long? ContentLength { get; set; }
            public override string ContentType { get; set; }

            public override IResponseCookies Cookies => throw new NotImplementedException();

            public override bool HasStarted => true;

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
    }
}
