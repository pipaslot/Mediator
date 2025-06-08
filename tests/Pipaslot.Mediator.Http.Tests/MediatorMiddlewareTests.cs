using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Http.Tests.Fakes;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests
{
    public class MediatorMiddlewareTests
    {
        private const string _request =
            "{\"Content\":{},\"Type\":\"Pipaslot.Mediator.Tests.ValidActions.NopRequest, Pipaslot.Mediator.Tests.ValidActions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}";

        private const string _message =
            "{\"Content\":{},\"Type\":\"Pipaslot.Mediator.Tests.ValidActions.NopMessage, Pipaslot.Mediator.Tests.ValidActions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}";

        [Fact]
        public async Task PostMessageWillBePropagatedToMediator()
        {
            await ExecuteMessage(new FakePostRequest(_message));
        }

        [Fact]
        public async Task PostRequestWillBePropagatedToMediator()
        {
            await ExecuteRequest(new FakePostRequest(_request));
        }

        [Fact]
        public async Task GetMessageWillBePropagatedToMediator()
        {
            await ExecuteMessage(new FakeGetRequest(_message));
        }

        [Fact]
        public async Task GetRequestWillBePropagatedToMediator()
        {
            await ExecuteRequest(new FakeGetRequest(_request));
        }

        private async Task ExecuteRequest(HttpRequest request)
        {
            var mediatorResponse = Task.FromResult((IMediatorResponse<string>)new MediatorResponse<string>(true, Array.Empty<object>()));
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(x => x.Execute<string>(It.IsAny<NopRequest>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
            var services = CreateServiceProvider(mediatorMock);
            var sut = services.GetRequiredService<MediatorMiddleware>();

            var context = new FakeContext(request, services);
            await sut.Invoke(context);

            mediatorMock.Verify(m => m.Execute(It.IsAny<NopRequest>(), It.IsAny<CancellationToken>()));
        }

        private async Task ExecuteMessage(HttpRequest request)
        {
            var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, Array.Empty<object>()));
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(x => x.Dispatch(It.IsAny<NopMessage>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);

            var services = CreateServiceProvider(mediatorMock);
            var sut = services.GetRequiredService<MediatorMiddleware>();

            var context = new FakeContext(request, services);
            await sut.Invoke(context);

            mediatorMock.Verify(m => m.Dispatch(It.IsAny<NopMessage>(), It.IsAny<CancellationToken>()));
        }

        private ServiceProvider CreateServiceProvider(Mock<IMediator> mediatorMock)
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddMediatorServer(o => o.SerializerType = SerializerType.V2)
                .AddActions([typeof(NopRequest), typeof(NopMessage)]);
            collection.AddScoped<MediatorMiddleware>();
            collection.AddScoped<RequestDelegate>(s => (c) => Task.CompletedTask);
            collection.AddSingleton<IMediator>(mediatorMock.Object);
            return collection.BuildServiceProvider();
        }
    }
}