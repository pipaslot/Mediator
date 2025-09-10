using Pipaslot.Mediator.Abstractions;
using System.Threading;

namespace Pipaslot.Mediator.Tests.E2E.ResultTypes;

public class NotNullBoolean
{
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Execute_ShouldPass(bool value)
    {
        var sut = Factory.CreateMediatorWithHandlers<FakeActionHandler>();
        var result = await sut.Execute(new FakeAction(value));
        Assert.True(result.Success);
        Assert.Equal(value, result.Result);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task ExecuteUnhandled_ShouldPass(bool value)
    {
        var sut = Factory.CreateMediatorWithHandlers<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(value));
        Assert.Equal(value, result);
    }

    public record FakeAction(bool Value) : IMediatorAction<bool>;

    public class FakeActionHandler : IMediatorHandler<FakeAction, bool>
    {
        public Task<bool> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            return Task.FromResult(action.Value);
        }
    }
}