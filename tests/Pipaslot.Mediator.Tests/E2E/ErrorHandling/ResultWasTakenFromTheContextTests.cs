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
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RemoveResultFromContextMiddleware>());
        var action = new RequestWithoutHandler();
        var result = await sut.Execute(action);
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_LogsOriginalExceptionDetailAtErrorLevel()
    {
        var (sut, logger) = Factory.CreateConfiguredMediatorWithLogger(c => c.Use<RemoveResultFromContextMiddleware>());

        await sut.Execute(new RequestWithoutHandler());

        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.IsType<MediatorMissingResultException>(entry.Exception);
    }

    [Fact]
    public async Task ExecuteUnhandled_ThrowMissingResultException()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RemoveResultFromContextMiddleware>());
        var action = new RequestWithoutHandler();
        var ex =
            await Assert.ThrowsAsync<MediatorMissingResultException>(async () =>
            {
                await sut.ExecuteUnhandled(action);
            });
        var context = Factory.FakeContext(action);
        Assert.Equal(MediatorMissingResultException.Create(typeof(RequestWithoutHandler.ResultDto), context).Message, ex.Message);
    }

    // Does not make sense for Dispatch and DispatchUnhandled
}