using Pipaslot.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ResultTypes;

public class NullableObject
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
    public async Task Execute_ReturnsNull_ShouldPass()
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.Execute(new FakeAction(true));
        Assert.True(result.Success, result.GetErrorMessage());
        Assert.Null(result.Result);
    }

    [Fact]
    public async Task ExecuteUnhandled_ReturnsValue_ShouldPass()
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(false));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteUnhandled_ReturnsNull_ShouldPass()
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(true));
        Assert.Null(result);
    }

    public record FakeAction(bool ReturnNull) : IMediatorAction<FakeResult?>;

    public record FakeResult;

    public class FakeActionHandler : IMediatorHandler<FakeAction, FakeResult?>
    {
        public Task<FakeResult?> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            var result = action.ReturnNull ? null : new FakeResult();
            return Task.FromResult(result);
        }
    }
}