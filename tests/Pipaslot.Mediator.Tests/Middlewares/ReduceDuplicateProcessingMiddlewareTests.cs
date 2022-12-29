using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests.Middlewares
{
    public class ReduceDuplicateProcessingMiddlewareTests
    {
        private readonly SemaphoreSlim _semaphore = new(0);
        async Task _next(MediatorContext res)
        {
            await _semaphore.WaitAsync();
            res.Results.Add(new object());
        }

        [Fact]
        public async void RunSingleAction_ShouldRunOnce()
        {
            var action = new FakeAction();

            var task = Run(action, out var ctx);
            _semaphore.Release();
            await task;
            Assert.Single(ctx.Results);
        }

        [Fact]
        public async void RunTheSameInstanceActionTwice_ShouldRunOnce()
        {
            var action = new FakeAction();

            var task1 = Run(action, out var ctx1);
            var task2 = Run(action, out var ctx2);
            _semaphore.Release(2);

            await Task.WhenAll(task1, task2);
            Assert.Single(ctx1.Results);
            Assert.Empty(ctx2.Results);
        }

        [Fact]
        public async void RunDuplicateTheSameTypeAction_ShouldRuntTwice()
        {
            var action = new FakeAction();

            var task1 = Run(action, out var ctx1);
            _semaphore.Release();
            await task1;
            var task2 = Run(action, out var ctx2);
            _semaphore.Release();

            await task2;
            Assert.Single(ctx1.Results);
            Assert.Single(ctx2.Results);
        }

        [Fact]
        public async void RunTwoTheSameTypeActionsWithTheSameHashCode_ShouldRunOnce()
        {
            var task1 = Run(new FakeAction() { Value = 1 }, out var ctx1);
            var task2 = Run(new FakeAction() { Value = 1 }, out var ctx2);
            _semaphore.Release(2);

            await Task.WhenAll(task1, task2);
            Assert.Single(ctx1.Results);
            Assert.Empty(ctx2.Results);
        }

        [Fact]
        public async void RunTwoDifferentActionsWithTheSameHashCode_ShouldRuntTwice()
        {
            var task1 = Run(new FakeAction() { Value = 1 }, out var ctx1);
            var task2 = Run(new FakeAction2() { Value = 1 }, out var ctx2);
            _semaphore.Release(2);

            await Task.WhenAll(task1, task2);
            Assert.Single(ctx1.Results);
            Assert.Single(ctx2.Results);
        }

        [Fact]
        public async void RunTwoDifferentActionsWithDifferentHashCode_ShouldRuntTwice()
        {
            var task1 = Run(new FakeAction() { Value = 1 },out var ctx1);
            var task2 = Run(new FakeAction2() { Value = 2 }, out var ctx2);
            _semaphore.Release(2);

            await Task.WhenAll(task1, task2);
            Assert.Single(ctx1.Results);
            Assert.Single(ctx2.Results);
        }

        [Fact]
        public async Task RaceCondition()
        {
            var mediator = Factory.CreateMediator(s => s.UseReduceDuplicateProcessing());
            var task1 = mediator.Execute(new RaceConditionAction() { Value = 1 });
            var task2 = mediator.Execute(new RaceConditionAction() { Value = 1 });

            await Task.WhenAll(task1, task2);
            var r1 = (await task1).Result;
            var r2 = (await task2).Result;

            Assert.True(r1 is RaceConditionResult);
            Assert.True(r2 is RaceConditionResult);
        }

        #region Setup

        private Task Run(IMediatorAction action, out MediatorContext context)
        {
            context = new MediatorContext(action, CancellationToken.None);
            var sut = new ReduceDuplicateProcessingMiddleware();
            return sut.Invoke(context, _next);
        }

        public class FakeAction : IMediatorAction
        {
            public int Value { get; set; }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        public class FakeAction2 : IMediatorAction
        {
            public int Value { get; set; }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        public record RaceConditionAction : IMediatorAction<RaceConditionResult>
        {
            public int Value { get; set; }
        }
        public class RaceConditionActionHandler : IMediatorHandler<RaceConditionAction, RaceConditionResult>
        {
            public async Task<RaceConditionResult> Handle(RaceConditionAction action, CancellationToken cancellationToken)
            {
                await Task.Delay(500);
                return new RaceConditionResult();
            }
        }
        public class RaceConditionResult
        {

        }
        #endregion

    }
}
