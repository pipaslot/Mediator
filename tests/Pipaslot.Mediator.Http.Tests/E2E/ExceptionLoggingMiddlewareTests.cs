using Pipaslot.Mediator.Tests.ValidActions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Tests;

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
        var services = Factory.CreateServiceProvider(c =>
            {
                c.AddActionsFromAssemblyOf<ExceptionLoggingMiddlewareTests>()
                    .AddActionsFromAssemblyOf<SingleHandler.Message>()
                    .AddHandlersFromAssemblyOf<ExceptionLoggingMiddlewareTests>()
                    .AddHandlersFromAssemblyOf<SingleHandler.MessageHandler>()
                    .UseExceptionLogging();
            }
        );
        return services.GetRequiredService<IMediator>();
    }
}