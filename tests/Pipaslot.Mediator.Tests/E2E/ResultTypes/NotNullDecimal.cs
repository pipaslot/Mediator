using Pipaslot.Mediator.Abstractions;
using System.Threading;

namespace Pipaslot.Mediator.Tests.E2E.ResultTypes;

public class NotNullDecimal
{
    [Test]
    [Arguments(-100)]
    [Arguments(0)]
    [Arguments(100)]
    public async Task Execute_ShouldPass(decimal value)
    {
        var sut = Factory.CreateMediatorWithHandlers<FakeActionHandler>();
        var result = await sut.Execute(new FakeAction(value));
        Assert.True(result.Success);
        Assert.Equal(value, result.Result);
    }

    [Test]
    [Arguments(-100)]
    [Arguments(0)]
    [Arguments(100)]
    public async Task ExecuteUnhandled_ShouldPass(decimal value)
    {
        var sut = Factory.CreateMediatorWithHandlers<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(value));
        Assert.Equal(value, result);
    }

    public record FakeAction(decimal Value) : IMediatorAction<decimal>;

    public class FakeActionHandler : IMediatorHandler<FakeAction, decimal>
    {
        public Task<decimal> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            return Task.FromResult(action.Value);
        }
    }
}