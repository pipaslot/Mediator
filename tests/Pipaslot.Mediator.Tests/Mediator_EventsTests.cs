using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class Mediator_EventsTests
    {
        private readonly SemaphoreSlim _handlerSemaphore = new(0);
        private readonly List<ActionStartedEventArgs> _started = new();
        private readonly List<ActionCompletedEventArgs> _completed = new();

        #region Basic flow

        [Fact]
        public void NewMediatorDoesNotFireAnsyActionStartedEvent()
        {
            Create();
            Assert.Empty(_started);
        }

        [Fact]
        public void NewMediatorDoesNotFireAnsyActionCompletedEvent()
        {
            Create();
            Assert.Empty(_completed);
        }

        [Fact]
        public async Task SemaphoreActionIsExcetutable()
        {
            var sut = Create();
            var task = sut.Dispatch(new SemaphoreAction());
            _handlerSemaphore.Release();
            var response = await task;
            Assert.True(response.Success);
        }

        [Fact]
        public async Task StartedEventIsCalledInTheBeginningOfMediatorExcecution()
        {
            var sut = Create();
            var task = sut.Dispatch(new SemaphoreAction());
            Assert.Single(_started);
            _handlerSemaphore.Release();
            await task;
        }

        [Fact]
        public async Task CompletedEventIsNotCalledInTheBeginningOfMediatorExcecution()
        {
            var sut = Create();
            var task = sut.Dispatch(new SemaphoreAction());
            Assert.Empty(_completed);
            _handlerSemaphore.Release();
            await task;
        }

        [Fact]
        public async Task CompletedEventIsCalledAfterMediatorExcecution()
        {
            var sut = Create();
            var task = sut.Dispatch(new SemaphoreAction());
            _handlerSemaphore.Release();
            await task;
            Assert.Single(_completed);
        }

        [Fact]
        public async Task StartedEventIsNotCalledAfterMediatorExcecution()
        {
            var sut = Create();
            var task = sut.Dispatch(new SemaphoreAction());
            _handlerSemaphore.Release();
            await task;
            Assert.Single(_started);
        }

        #endregion

        [Fact]
        public async Task SendingTheSameActionInstanceTwiceFiresTwoStartedEvents()
        {
            var sut = Create();
            var action = new SemaphoreAction();
            var task1 = sut.Dispatch(action);
            var task2 = sut.Dispatch(action);
            Assert.Equal(2, _started.Count);
            _handlerSemaphore.Release(2);
            await Task.Delay(10); // Wait for event propagation
            await Task.WhenAll(task1, task2);
        }
        [Fact]
        public async Task SendingTheSameActionInstanceTwiceFiresTwoCompletedEvents()
        {
            var sut = Create();
            var action = new SemaphoreAction();
            var task1 = sut.Dispatch(action);
            var task2 = sut.Dispatch(action);
            _handlerSemaphore.Release(2);
            await Task.WhenAll(task1, task2);
            await Task.Delay(10); // Wait for event propagation
            Assert.Equal(2, _completed.Count);
        }

        #region Other cases

        #endregion

        #region Setup

        private IMediator Create()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddMediator()
                .AddActionsFromAssemblyOf<Factory>()
                .AddDefaultPipeline();
            collection.AddTransient<IMediatorHandler<SemaphoreAction>>(s => new SemaphoreHandler(_handlerSemaphore));
            var services = collection.BuildServiceProvider();

            var mediator = services.GetRequiredService<IMediator>();

            mediator.ActionStarted += OnStarted;
            mediator.ActionCompleted += OnCompleted;
            return mediator;
        }

        private void OnStarted(object sender, ActionStartedEventArgs args)
        {
            _started.Add(args);
        }

        private void OnCompleted(object sender, ActionCompletedEventArgs args)
        {
            _completed.Add(args);
        }

        public class SemaphoreAction : IMediatorAction
        {
        }

        public class SemaphoreHandler : IMediatorHandler<SemaphoreAction>
        {
            private readonly SemaphoreSlim _semaphore;

            public SemaphoreHandler(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public async Task Handle(SemaphoreAction action, CancellationToken cancellationToken)
            {
                await _semaphore.WaitAsync(cancellationToken);
            }
        }

        #endregion

    }
}
