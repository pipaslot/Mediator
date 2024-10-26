using Pipaslot.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ResultTypes;

public class NotNullInteger
{
    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(100)]
    public async Task Execute_ShouldPass(int value)
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
    public async Task ExecuteUnhandled_ShouldPass(int value)
    {
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(value));
        Assert.Equal(value, result);
    }

    public record FakeAction(int Value) : IMediatorAction<int>;

    public class FakeActionHandler : IMediatorHandler<FakeAction, int>
    {
        public Task<int> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            return Task.FromResult(action.Value);
        }
    }
}