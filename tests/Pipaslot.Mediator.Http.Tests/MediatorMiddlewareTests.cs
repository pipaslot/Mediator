using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
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
        private const string Request = "{\"Content\":{},\"Type\":\"Pipaslot.Mediator.Tests.ValidActions.NopRequest, Pipaslot.Mediator.Tests.ValidActions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}";
        private const string Message = "{\"Content\":{},\"Type\":\"Pipaslot.Mediator.Tests.ValidActions.NopMessage, Pipaslot.Mediator.Tests.ValidActions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}";

        [Fact]
        public async Task PostMessageWillBeProapgatedToMediator()
        {
            await ExecuteMessage(new PostRequest(Message));
        }
        [Fact]
        public async Task PostRequestWillBeProapgatedToMediator()
        {
            await ExecuteRequest(new PostRequest(Request));
        }
        [Fact]
        public async Task GetMessageWillBeProapgatedToMediator()
        {
            await ExecuteMessage(new GetRequest(Message));
        }
        [Fact]
        public async Task GetRequestWillBeProapgatedToMediator()
        {
            await ExecuteRequest(new GetRequest(Request));
        }

        private async Task ExecuteRequest(HttpRequest request)
        {
            var mediatorResponse = Task.FromResult((IMediatorResponse<string>)new MediatorResponse<string>(true, Array.Empty<object>()));
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(x => x.Execute<string>(It.IsAny<NopRequest>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddMediatorServer(o => o.SerializerType = SerializerType.V2)
                .AddActionsFromAssemblyOf<NopRequest>();
            collection.AddScoped<MediatorMiddleware>();
            collection.AddScoped<RequestDelegate>(s => (c) => Task.CompletedTask);
            collection.AddSingleton<IMediator>(mediatorMock.Object);
            var services = collection.BuildServiceProvider();
            var sut = services.GetRequiredService<MediatorMiddleware>();

            var context = new FakeContext(request);
            context.RequestServices = services;
            await sut.Invoke(context);

            mediatorMock.Verify(m => m.Execute(It.IsAny<NopRequest>(), It.IsAny<CancellationToken>()));
        }

        private async Task ExecuteMessage(HttpRequest request)
        {
            var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, Array.Empty<object>()));
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(x => x.Dispatch(It.IsAny<NopMessage>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddMediatorServer(o => o.SerializerType = SerializerType.V2)
                .AddActionsFromAssemblyOf<NopMessage>();
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
                RequestServices = new Mock<IServiceProvider>().Object;
            }

            public override IFeatureCollection Features => throw new NotImplementedException();

            public override HttpRequest Request => _request;

            public override HttpResponse Response => new FakeResponse();

            public override ConnectionInfo Connection => throw new NotImplementedException();

            public override WebSocketManager WebSockets => throw new NotImplementedException();
            [Obsolete]
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
            public PostRequest(string action)
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
            public override IQueryCollection Query { get; set; }
            public override Stream Body { get; set; } = new Mock<Stream>().Object;

            public GetRequest(string action) : base("")
            {
                var query = new QueryCollection();
                query.Add(new KeyValuePair<string, StringValues>("action",
                    new StringValues(action)
                    ));
                Query = query;
            }
#pragma warning disable CS8644 // Type does not implement interface member. Nullability of reference types in interface implemented by the base type doesn't match.
            private class QueryCollection : List<KeyValuePair<string, StringValues>>, IQueryCollection
#pragma warning restore CS8644 // Type does not implement interface member. Nullability of reference types in interface implemented by the base type doesn't match.
            {
                public StringValues this[string key] => this.FirstOrDefault(v => v.Key == key).Value;

                public ICollection<string> Keys => throw new NotImplementedException();

                public bool ContainsKey(string key)
                {
                    return this.Any(v => v.Key == key);
                }

                public bool TryGetValue(string key, out StringValues value)
                {
                    var a = this.FirstOrDefault(v => v.Key == key);
                    if (string.IsNullOrEmpty(a.Key))
                    {
                        value = default;
                        return false;
                    }
                    value = a.Value;
                    return true;
                }
            }
        }
        private class FakeResponse : HttpResponse
        {
            public override HttpContext HttpContext => throw new NotImplementedException();

            public override int StatusCode { get; set; } = 200;

            public override IHeaderDictionary Headers => throw new NotImplementedException();

            public override Stream Body { get; set; } = new Mock<Stream>().Object;
            public override long? ContentLength { get; set; }
            public override string ContentType { get; set; } = string.Empty;

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
