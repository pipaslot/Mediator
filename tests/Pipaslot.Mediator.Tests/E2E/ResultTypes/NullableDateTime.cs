using Pipaslot.Mediator.Abstractions;
using System;
using System.Threading;

namespace Pipaslot.Mediator.Tests.E2E.ResultTypes;

public class NullableDateTime
{
    [Test]
    public async Task Execute_ReturnsValue_ShouldPass()
    {
        var value = new DateTime(2020, 01, 01);
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
    public async Task ExecuteUnhandled_ReturnsValue_ShouldPass()
    {
        var value = new DateTime(2020, 01, 01);
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

    public record FakeAction(DateTime? Value) : IMediatorAction<DateTime?>;

    public class FakeActionHandler : IMediatorHandler<FakeAction, DateTime?>
    {
        public Task<DateTime?> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            return Task.FromResult(action.Value);
        }
    }
}