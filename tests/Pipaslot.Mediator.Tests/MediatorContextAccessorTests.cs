using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

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
            collection.AddTransient<IMediatorHandler<RootAction>, RootActionHandler>();
            collection.AddTransient<IMediatorHandler<NestedAction>, NestedActionHandler>();
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
            await _mediator.DispatchUnhandled(new RootAction(RootActionTestCase.SingleNested));
            Assert.Null(_contextAccessor.Context);
            Assert.Empty(_contextAccessor.ContextStack);
        }

        [Fact]
        public async Task Flow()
        {
            _service.AssertZero();
            await _mediator.DispatchUnhandled(new RootAction(RootActionTestCase.SingleNested));
            _service.AssertZero();
        }

        [Fact]
        public async Task NestedParallelTask_KnownFailingCase()
        {
            // AsyncLocal accessor causes issues because all the action are pushed into one context stack, instead of being separated
            // We do not have solution at the moment
            // The only workaround is to access the context before first await in the called handler
            // TODO: Prevent this issue and solve the exception 
            await Assert.ThrowsAsync<EqualException>(async () =>
            {
                await _mediator.DispatchUnhandled(new RootAction(RootActionTestCase.ConcurrentNested));
            });
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
                Assert.Equal(typeof(RootAction), _accessor.Context?.Action?.GetType());
                Assert.Single(_accessor.ContextStack);
                Assert.Equal(typeof(RootAction), _accessor.ContextStack.First().Action.GetType());
                // Verify that helper classes returns the same result as well
                Assert.Equal(typeof(RootAction), _accessor.GetRootContext()?.Action?.GetType());
                Assert.Empty(_accessor.GetParentContexts());
            }

            public void AssertTwo()
            {
                Assert.Equal(typeof(NestedAction), _accessor.Context?.Action?.GetType());
                Assert.Equal(2, _accessor.ContextStack.Count);
                Assert.Equal(typeof(NestedAction), _accessor.ContextStack.First().Action.GetType());
                Assert.Equal(typeof(RootAction), _accessor.ContextStack.Skip(1).First().Action.GetType());
                // Verify that helper classes returns the same result as well
                Assert.Equal(typeof(RootAction), _accessor.GetRootContext()?.Action?.GetType());
                Assert.Single(_accessor.GetParentContexts());
                Assert.Equal(typeof(RootAction), _accessor.GetParentContexts()?.First()?.Action?.GetType());
            }
        }

        private enum RootActionTestCase
        {
            SingleNested,
            ConcurrentNested
        }

        private record RootAction(RootActionTestCase Case) : IMediatorAction;

        private class RootActionHandler : IMediatorHandler<RootAction>
        {
            private readonly FakeService _service;
            private readonly IMediator _mediator;

            public RootActionHandler(FakeService service, IMediator mediator)
            {
                _service = service;
                _mediator = mediator;
            }

            public async Task Handle(RootAction action, CancellationToken cancellationToken)
            {
                _service.AssertSingle();
                if (action.Case == RootActionTestCase.SingleNested)
                {
                    await _mediator.DispatchUnhandled(new NestedAction());
                }
                else
                {
                    var actions = new[]
                    {
                        new NestedAction(TimeSpan.FromMilliseconds(200)), 
                        new NestedAction(TimeSpan.FromMilliseconds(100)), 
                        new NestedAction(TimeSpan.FromMilliseconds(50)), 
                        new NestedAction(TimeSpan.FromMilliseconds(20)),
                        new NestedAction(TimeSpan.FromMilliseconds(10)),
                    };
                    // Test concurrency
                    var tasks = actions.Select(async a => await _mediator.DispatchUnhandled(a));
                    await Task.WhenAll(tasks);
                }

                _service.AssertSingle();
            }
        }

        private record NestedAction(TimeSpan? Delay = null) : IMediatorAction;

        private class NestedActionHandler : IMediatorHandler<NestedAction>
        {
            private readonly FakeService _service;

            public NestedActionHandler(FakeService service)
            {
                _service = service;
            }

            public async Task Handle(NestedAction action, CancellationToken cancellationToken)
            {
                if (action.Delay.HasValue)
                {
                    await Task.Delay(action.Delay.Value, cancellationToken);
                }
                _service.AssertTwo();
            }
        }
    }
}