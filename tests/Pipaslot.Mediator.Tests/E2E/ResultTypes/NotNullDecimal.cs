using Pipaslot.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ResultTypes;

public class NotNullDecimal
{
    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(100)]
    public async Task Execute_ShouldPass(decimal value)
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.Execute(new FakeAction(value));
        Assert.True(result.Success);
        Assert.Equal(value, result.Result);
    }
    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(100)]
    public async Task ExecuteUnhandled_ShouldPass(decimal value)
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
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