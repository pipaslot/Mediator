using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Pipaslot.Mediator.Http.Tests.E2E;

public class ExceptionLoggingMiddlewareTests
{
    [Fact]
    public async Task Execute_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsSuccessFalse()
    {
        var sut = CreateMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Execute_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsErrorMessage()
    {
        var sut = CreateMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task ExecuteUnhandled_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsException()
    {
        var sut = CreateMediator();
        await Assert.ThrowsAsync<SingleHandler.RequestException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(false));
        });
    }

    [Fact]
    public async Task Dispatch_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsSuccessFalse()
    {
        var sut = CreateMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Dispatch_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsErrorMessage()
    {
        var sut = CreateMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task DispatchUnhandled_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsException()
    {
        var sut = CreateMediator();
        await Assert.ThrowsAsync<SingleHandler.MessageException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(false));
        });
    }

    private static IMediator CreateMediator()
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddMediator()
            .AddActionsFromAssemblyOf<SingleHandler.Message>()
            .AddHandlersFromAssemblyOf<SingleHandler.MessageHandler>()
            .UseExceptionLogging();
        var services = collection.BuildServiceProvider();
        return services.GetRequiredService<IMediator>();
    }
    
    
    public static class SingleHandler
    {
        public record Request(bool Pass) : IRequest<Response>;

        public class Response;

        public record Message(bool Pass) : IMessage;

        public class RequestException() : Exception("Requesthandler failed");

        public class MessageException() : Exception("Message handler failed");

        public class RequestHandler : IRequestHandler<Request, Response>
        {
            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                return !request.Pass ? throw new RequestException() : Task.FromResult(new Response());
            }
        }

        public class MessageHandler : IMessageHandler<Message>
        {
            public Task Handle(Message request, CancellationToken cancellationToken)
            {
                return !request.Pass ? throw new MessageException() : Task.CompletedTask;
            }
        }
    }
}