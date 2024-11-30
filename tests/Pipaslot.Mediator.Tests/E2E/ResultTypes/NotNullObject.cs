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
    public async Task Execute_ReturnsNull_ShouldFailButSuccess()
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var action = new FakeAction(true);
        var result = await sut.Execute(action);
        Assert.True(result
            .Success); // Would be nice to get false and detect if null was provided when null is not expected, but we can not achieve it in the current C# version
    }

    [Fact]
    public async Task ExecuteUnhandled_ReturnsValue_ShouldPass()
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(false));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteUnhandled_ReturnsNull_ShouldFailButSuccess()
    {
        var action = new FakeAction(true);
        // Would be nice to get failure and detect if null was provided when null is not expected, but we can not achieve it in the current C# version
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        await sut.ExecuteUnhandled(action);
    }

    public record FakeAction(bool ReturnNull) : IMediatorAction<FakeResult>;

    public record FakeResult;

    public class FakeActionHandler : IMediatorHandler<FakeAction, FakeResult>
    {
        public Task<FakeResult> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            var result = action.ReturnNull ? null : new FakeResult();
            return Task.FromResult(result!);
        }
    }
}