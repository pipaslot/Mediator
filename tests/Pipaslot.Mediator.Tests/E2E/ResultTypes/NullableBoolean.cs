using Pipaslot.Mediator.Abstractions;
using System.Threading;

namespace Pipaslot.Mediator.Tests.E2E.ResultTypes;

public class NullableBoolean
{
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Execute_ReturnsValue_ShouldPass(bool value)
    {
        var sut = Factory.CreateMediatorWithHandlers<FakeActionHandler>();
        var result = await sut.Execute(new FakeAction(value));
        Assert.True(result.Success);
        Assert.NotNull(result.Result);
        Assert.Equal(value, result.Result!.Value);
    }

    [Test]
    public async Task Execute_ReturnsNull_ShouldPass()
    {
        var sut = Factory.CreateMediatorWithHandlers<FakeActionHandler>();
        var result = await sut.Execute(new FakeAction(null));
        Assert.True(result.Success, result.GetErrorMessage());
        Assert.Null(result.Result);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ExecuteUnhandled_ReturnsValue_ShouldPass(bool value)
    {
        var sut = Factory.CreateMediatorWithHandlers<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(value));
        Assert.Equal(value, result!.Value);
    }

    [Test]
    public async Task ExecuteUnhandled_ReturnsNull_ShouldPass()
    {
        var sut = Factory.CreateMediatorWithHandlers<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(null));
        Assert.Null(result);
    }


    public record FakeAction(bool? Value) : IMediatorAction<bool?>;

    public class FakeActionHandler : IMediatorHandler<FakeAction, bool?>
    {
        public Task<bool?> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            return Task.FromResult(action.Value);
        }
    }
}