using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Tests.E2E.Fixtures;
using Pipaslot.Mediator.Tests.InvalidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ErrorHandling;

public class ResultWasTakenFromTheContextTests
{
    [Fact]
    public async Task Execute_FailWithGenericErrorBecauseNoHandlerIsConfigured()
    {
        var sut = CreateMediator();
        var action = new RequestWithoutHandler();
        var result = await sut.Execute(action);
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_LogsOriginalExceptionDetailAtErrorLevel()
    {
        var (sut, logger) = CreateMediatorWithLogger();

        await sut.Execute(new RequestWithoutHandler());

        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.IsType<MediatorMissingResultException>(entry.Exception);
    }

    [Fact]
    public async Task ExecuteUnhandled_ThrowMissingResultException()
    {
        var sut = CreateMediator();
        var action = new RequestWithoutHandler();
        var ex =
            await Assert.ThrowsAsync<MediatorMissingResultException>(async () =>
            {
                await sut.ExecuteUnhandled(action);
            });
        Assert.Contains(nameof(RequestWithoutHandler.ResultDto), ex.Message);
    }

    // Does not make sense for Dispatch and DispatchUnhandled

    private IMediator CreateMediator()
    {
        return CreateMediatorWithLogger().Mediator;
    }

    private (IMediator Mediator, TestLogger<Mediator> Logger) CreateMediatorWithLogger()
    {
        return Factory.CreateMediatorWithLogger(c => c.Use<RemoveResultFromContextMiddleware>());
    }
}