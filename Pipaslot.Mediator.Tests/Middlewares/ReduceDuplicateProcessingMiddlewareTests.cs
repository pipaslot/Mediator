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
        private readonly MediatorContext _context = new();
        async Task _next(MediatorContext res)
        {
            await _semaphore.WaitAsync();
            res.Results.Add(new object());
        }

        [Fact]
        public async void RunSingleAction_ShouldRunOnce()
        {
            var action = new FakeAction();

            var task = Run(action);
            _semaphore.Release();
            await task;
            Assert.Single(_context.Results);
        }

        [Fact]
        public async void RunTheSameInstanceActionTwice_ShouldRunOnce()
        {
            var action = new FakeAction();

            var task1 = Run(action);
            var task2 = Run(action);
            _semaphore.Release(2);

            await Task.WhenAll(task1, task2);
            Assert.Single(_context.Results);
        }

        [Fact]
        public async void RunDuplicateTheSameTypeAction_ShouldRuntTwice()
        {
            var action = new FakeAction();

            var task1 = Run(action);
            _semaphore.Release();
            await task1;
            var task2 = Run(action);
            _semaphore.Release();

            await task2;
            Assert.Equal(2, _context.Results.Count);
        }

        [Fact]
        public async void RunTwoTheSameTypeActionsWithTheSameHashCode_ShouldRunOnce()
        {
            var task1 = Run(new FakeAction() { Value = 1 });
            var task2 = Run(new FakeAction() { Value = 1 });
            _semaphore.Release(2);

            await Task.WhenAll(task1, task2);
            Assert.Single(_context.Results);
        }

        [Fact]
        public async void RunTwoDifferentActionsWithTheSameHashCode_ShouldRuntTwice()
        {
            var task1 = Run(new FakeAction() { Value = 1 });
            var task2 = Run(new FakeAction2() { Value = 1 });
            _semaphore.Release(2);

            await Task.WhenAll(task1, task2);
            Assert.Equal(2, _context.Results.Count);
        }

        [Fact]
        public async void RunTwoDifferentActionsWithDifferentHashCode_ShouldRuntTwice()
        {
            var task1 = Run(new FakeAction() { Value = 1 });
            var task2 = Run(new FakeAction2() { Value = 2 });
            _semaphore.Release(2);

            await Task.WhenAll(task1, task2);
            Assert.Equal(2, _context.Results.Count);
        }

        #region Setup

        private Task Run<TAction>(TAction action)
        {
            var sut = new DuplicatesReductionMiddleware();
            return sut.Invoke(action, _context, _next, CancellationToken.None);
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

        #endregion

    }
}
