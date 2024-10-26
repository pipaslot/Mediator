using Pipaslot.Mediator.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ResultTypes;

public class NotNullDateTime
{
    [Theory]
    [InlineData("2021-09-01")]
    public async Task Execute_ShouldPass(string dateString)
    {
        var value = DateTime.Parse(dateString);
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.Execute(new FakeAction(value));
        Assert.True(result.Success);
        Assert.Equal(value, result.Result);
    }

    [Theory]
    [InlineData("2021-09-01")]
    public async Task ExecuteUnhandled_ShouldPass(string dateString)
    {
        var value = DateTime.Parse(dateString);
        var sut = Factory.CreateConfiguredMediator<FakeActionHandler>();
        var result = await sut.ExecuteUnhandled(new FakeAction(value));
        Assert.Equal(value, result);
    }

    public record FakeAction(DateTime Value) : IMediatorAction<DateTime>;

    public class FakeActionHandler : IMediatorHandler<FakeAction, DateTime>
    {
        public Task<DateTime> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            return Task.FromResult(action.Value);
        }
    }
}