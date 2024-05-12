using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests
{
    public class MediatorContextAccessorTests
    {
        private readonly IMediator _mediator;
        private readonly IMediatorContextAccessor _contextAccessor;
        private readonly FakeService _service;

        public MediatorContextAccessorTests()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddMediator()
                .AddActionsFromAssemblyOf<Factory>()
                .UseActionEvents();
            collection.AddScoped<FakeService>();
            collection.AddTransient<IMediatorHandler<Level1Action>, Level1ActionHandler>();
            collection.AddTransient<IMediatorHandler<Level2Action>, Level2ActionHandler>();
            collection.AddTransient<IMediatorHandler<Level3Action>, Level3ActionHandler>();
            var services = collection.BuildServiceProvider();

            _mediator = services.GetRequiredService<IMediator>();
            _contextAccessor = services.GetRequiredService<IMediatorContextAccessor>();
            _service = services.GetRequiredService<FakeService>();
        }

        [Fact]
        public void NoAction_ContextIsNull()
        {
            Assert.Null(_contextAccessor.Context);
            Assert.Empty(_contextAccessor.ContextStack);
        }

        [Fact]
        public async Task ExecutionCompleted_ContextIsNull()
        {
            await _mediator.DispatchUnhandled(new Level1Action(ActionBehaviorTestCase.SingleNested));
            Assert.Null(_contextAccessor.Context);
            Assert.Empty(_contextAccessor.ContextStack);
        }

        [Fact]
        public async Task Flow()
        {
            _service.AssertZero();
            await _mediator.DispatchUnhandled(new Level1Action(ActionBehaviorTestCase.SingleNested));
            _service.AssertZero();
        }

        [Fact]
        public async Task NestedParallelTask_KnownFailingCase()
        {
            await _mediator.DispatchUnhandled(new Level1Action(ActionBehaviorTestCase.ConcurrentNested));
        }

        private class FakeService
        {
            private readonly IMediatorContextAccessor _accessor;

            public FakeService(IMediatorContextAccessor accessor)
            {
                _accessor = accessor;
            }

            public void AssertZero()
            {
                Assert.Empty(_accessor.ContextStack);
                Assert.Null(_accessor.Context);
                // Verify that helper classes returns the same result as well
                Assert.Null(_accessor.GetRootContext());
                Assert.Empty(_accessor.GetParentContexts());
            }

            public void AssertSingle()
            {
                Assert.Equal(typeof(Level1Action), _accessor.Context?.Action?.GetType());
                Assert.Single(_accessor.ContextStack);
                // Verify that helper classes returns the same result as well
                Assert.Equal(typeof(Level1Action), _accessor.GetRootContext()?.Action?.GetType());
                Assert.Empty(_accessor.GetParentContexts());
            }

            public void AssertTwo()
            {
                Assert.Equal(typeof(Level2Action), _accessor.Context?.Action?.GetType());
                Assert.Equal(2, _accessor.ContextStack.Count);
                // Verify that helper classes returns the same result as well
                Assert.Equal(typeof(Level1Action), _accessor.GetRootContext()?.Action?.GetType());
                Assert.Single(_accessor.GetParentContexts());
                Assert.Equal(typeof(Level1Action), _accessor.GetParentContexts()?.First()?.Action?.GetType());
            }

            public void AssertThree()
            {
                Assert.Equal(typeof(Level3Action), _accessor.Context?.Action?.GetType());
                Assert.Equal(3, _accessor.ContextStack.Count);
                // Verify that helper classes returns the same result as well
                Assert.Equal(typeof(Level1Action), _accessor.GetRootContext()?.Action?.GetType());
                Assert.Equal(2, _accessor.GetParentContexts().Length);
                Assert.Equal(typeof(Level2Action), _accessor.GetParentContexts()?.First()?.Action?.GetType());
            }
        }

        private enum ActionBehaviorTestCase
        {
            SingleNested,
            ConcurrentNested
        }

        /// <summary>
        /// Entry level action calling nested mediator actions
        /// </summary>
        /// <param name="Case"></param>
        private record Level1Action(ActionBehaviorTestCase Case) : IMediatorAction;

        private class Level1ActionHandler : IMediatorHandler<Level1Action>
        {
            private readonly FakeService _service;
            private readonly IMediator _mediator;

            public Level1ActionHandler(FakeService service, IMediator mediator)
            {
                _service = service;
                _mediator = mediator;
            }

            public async Task Handle(Level1Action action, CancellationToken cancellationToken)
            {
                _service.AssertSingle();
                if (action.Case == ActionBehaviorTestCase.SingleNested)
                {
                    await _mediator.DispatchUnhandled(new Level2Action(action.Case), cancellationToken);
                }
                else if (action.Case == ActionBehaviorTestCase.ConcurrentNested)
                {
                    var actions = new[]
                    {
                        new Level2Action(action.Case, TimeSpan.FromMilliseconds(50)),
                        new Level2Action(action.Case, TimeSpan.FromMilliseconds(20)),
                        new Level2Action(action.Case, TimeSpan.FromMilliseconds(10)),
                    };
                    var tasks = actions.Select(async a => await _mediator.DispatchUnhandled(a, cancellationToken));
                    await Task.WhenAll(tasks);
                }
                else
                {
                    throw new NotImplementedException();
                }

                _service.AssertSingle();
            }
        }

        /// <summary>
        /// Action executed by Level 1
        /// </summary>
        /// <param name="Delay"></param>
        private record Level2Action(ActionBehaviorTestCase Case, TimeSpan? Delay = null) : IMediatorAction;

        private class Level2ActionHandler : IMediatorHandler<Level2Action>
        {
            private readonly FakeService _service;
            private readonly IMediator _mediator;

            public Level2ActionHandler(FakeService service, IMediator mediator)
            {
                _service = service;
                _mediator = mediator;
            }

            public async Task Handle(Level2Action action, CancellationToken cancellationToken)
            {
                _service.AssertTwo();
                if (action.Delay.HasValue)
                {
                    await Task.Delay(action.Delay.Value, cancellationToken);
                }

                if (action.Case == ActionBehaviorTestCase.SingleNested)
                {
                    await _mediator.DispatchUnhandled(new Level3Action(), cancellationToken);
                }
                else if (action.Case == ActionBehaviorTestCase.ConcurrentNested)
                {
                    var actions = new[]
                    {
                        new Level3Action(TimeSpan.FromMilliseconds(50)), new Level3Action(TimeSpan.FromMilliseconds(20)),
                        new Level3Action(TimeSpan.FromMilliseconds(10)),
                    };
                    var tasks = actions.Select(a => _mediator.DispatchUnhandled(a, cancellationToken));
                    await Task.WhenAll(tasks);
                }
                else
                {
                    throw new NotImplementedException();
                }

                _service.AssertTwo();
            }
        }

        /// <summary>
        /// Action executed by Level 2
        /// </summary>
        /// <param name="Delay"></param>
        private record Level3Action(TimeSpan? Delay = null) : IMediatorAction;

        private class Level3ActionHandler : IMediatorHandler<Level3Action>
        {
            private readonly FakeService _service;

            public Level3ActionHandler(FakeService service)
            {
                _service = service;
            }

            public async Task Handle(Level3Action action, CancellationToken cancellationToken)
            {
                _service.AssertThree();
                if (action.Delay.HasValue)
                {
                    await Task.Delay(action.Delay.Value, cancellationToken);
                }

                _service.AssertThree();
            }
        }
    }
}