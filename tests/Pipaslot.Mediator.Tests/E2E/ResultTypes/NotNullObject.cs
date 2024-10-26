using Pipaslot.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ResultTypes;

public class NotNullObject
{
    [Fact]
    public async Task Execute_ReturnsValue_ShouldPass()
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.Execute(new FakeAction(false));
        Assert.True(result.Success);
        Assert.NotNull(result.Result);
    }

    [Fact]
    public async Task Execute_ReturnsNull_ShouldFail()
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var action = new FakeAction(true);
        var result = await sut.Execute(action);
        Assert.False(result.Success, "The null appeared unexpectedly. The annotation declares not NULL");
        var expectedMessage = MediatorExecutionException.CreateForMissingResult(Factory.FakeContext(action), typeof(FakeResult)).Message;
        Assert.Equal(expectedMessage, result.GetErrorMessage());
    }
    
    [Fact]
    public async Task ExecuteUnhandled_ReturnsValue_ShouldPass()
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(false));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteUnhandled_ReturnsNull_ShouldFail()
    {
        var action = new FakeAction(true);
        var expectedMessage = MediatorExecutionException.CreateForMissingResult(Factory.FakeContext(action), typeof(FakeResult)).Message;
        var ex = await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
        {
            var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
            await sut.ExecuteUnhandled(action);
        });
        
        Assert.Equal(expectedMessage, ex.Message);
    }

    public record FakeAction(bool ReturnNull) : IMediatorAction<FakeResult>;

    public record FakeResult
    {
    };

    public class FakeActionHandler : IMediatorHandler<FakeAction, FakeResult>
    {
        public Task<FakeResult> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            var result = action.ReturnNull ? null : new FakeResult();
            return Task.FromResult(result!);
        }
    }
}