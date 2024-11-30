using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Middlewares;

public class ReduceDuplicateProcessingMiddlewareTests
{
    [Fact]
    public async Task RunSingleAction_ShouldRunOnce()
    {
        var mediator = Factory.CreateConfiguredMediator(s =>
        {
            s.Use<ReduceDuplicateProcessingMiddleware>();
        });
        var res = await mediator.Execute(new FakeAction() { Value = 1 });

        Assert.True(res.Result.Number > 0);
    }

    [Fact]
    public async Task RunTheSameInstanceActionTwice_ShouldRunOnce()
    {
        var mediator = Factory.CreateConfiguredMediator(s =>
        {
            s.Use<ReduceDuplicateProcessingMiddleware>();
        });
        var res1 = await mediator.Execute(new FakeAction() { Value = 1 });
        var res2 = await mediator.Execute(new FakeAction() { Value = 1 });

        Assert.NotEqual(res1.Result.Number, res2.Result.Number);
    }

    [Fact]
    public async Task RunDuplicateTheSameTypeAction_ShouldRuntTwice()
    {
        var action = new FakeAction() { Value = 1 };
        var mediator = Factory.CreateConfiguredMediator(s =>
        {
            s.Use<ReduceDuplicateProcessingMiddleware>();
        });
        var res1 = await mediator.Execute(action);
        var res2 = await mediator.Execute(action);

        Assert.NotEqual(res1.Result.Number, res2.Result.Number);
    }

    [Fact]
    public async Task RunTwoTheSameTypeActionsWithTheSameHashCode_ShouldRunOnce()
    {
        var mediator = Factory.CreateConfiguredMediator(s =>
        {
            s.Use<ReduceDuplicateProcessingMiddleware>();
        });
        var task1 = mediator.Execute(new FakeAction() { Value = 1 });
        var task2 = mediator.Execute(new FakeAction() { Value = 1 });

        await Task.WhenAll(task1, task2);
        var res1 = await task1;
        var res2 = await task2;

        Assert.Equal(res1.Result.Number, res2.Result.Number);
    }

    [Fact]
    public async Task RunTwoDifferentActionsWithTheSameHashCode_ShouldRuntTwice()
    {
        var mediator = Factory.CreateConfiguredMediator(s =>
        {
            s.Use<ReduceDuplicateProcessingMiddleware>();
        });
        var task1 = mediator.Execute(new FakeAction() { Value = 1 });
        var task2 = mediator.Execute(new FakeAction2() { Value = 1 });

        await Task.WhenAll(task1, task2);
        var res1 = await task1;
        var res2 = await task2;

        Assert.NotEqual(res1.Result.Number, res2.Result.Number);
    }

    [Fact]
    public async Task RunTwoDifferentActionsWithDifferentHashCode_ShouldRuntTwice()
    {
        var mediator = Factory.CreateConfiguredMediator(s =>
        {
            s.Use<ReduceDuplicateProcessingMiddleware>();
        });
        var task1 = mediator.Execute(new FakeAction() { Value = 1 });
        var task2 = mediator.Execute(new FakeAction2() { Value = 2 });

        await Task.WhenAll(task1, task2);
        var res1 = await task1;
        var res2 = await task2;

        Assert.NotEqual(res1.Result.Number, res2.Result.Number);
    }

    #region Setup

    public class FakeAction : IMediatorAction<FakeActionResult>
    {
        public int Value { get; set; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class FakeActionHandler : IMediatorHandler<FakeAction, FakeActionResult>
    {
        private static int Iterator = 1;

        public async Task<FakeActionResult> Handle(FakeAction action, CancellationToken cancellationToken)
        {
            await Task.Delay(100);
            Iterator++;
            return new FakeActionResult(Iterator);
            ;
        }
    }

    public class FakeAction2 : IMediatorAction<FakeActionResult>
    {
        public int Value { get; set; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class FakeAction2Handler : IMediatorHandler<FakeAction2, FakeActionResult>
    {
        private static int Iterator = 1000;

        public async Task<FakeActionResult> Handle(FakeAction2 action, CancellationToken cancellationToken)
        {
            await Task.Delay(100);
            Iterator++;
            return new FakeActionResult(Iterator);
            ;
        }
    }

    public record FakeActionResult(int Number);

    #endregion
}