using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests
{
    public class MediatorContextAccessorTests
    {
        private IMediator _mediator;
        private IMediatorContextAccessor _contextAccessor;
        private FakeService _service;

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
            await _mediator.Dispatch(new RootAction());
            Assert.Null(_contextAccessor.Context);
            Assert.Empty(_contextAccessor.ContextStack);
        }

        [Fact]
        public async Task Flow()
        {
            _service.AssertZero();
            await _mediator.Dispatch(new RootAction());
            _service.AssertZero();
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
            }

            public void AssertSingle()
            {
                Assert.Equal(typeof(RootAction), _accessor.Context.GetType());
                Assert.Single(_accessor.ContextStack);
                Assert.Equal(typeof(RootAction), _accessor.ContextStack.First().GetType());
            }

            public void AssertTwo()
            {
                Assert.Equal(typeof(RootAction), _accessor.Context.GetType());
                Assert.Equal(2,_accessor.ContextStack.Count);
                Assert.Equal(typeof(RootAction), _accessor.ContextStack.First().GetType());
                Assert.Equal(typeof(NestedAction), _accessor.ContextStack.Skip(1).First().GetType());
            }
        }

        private class RootAction : IMediatorAction
        {

        }
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
                await _mediator.Dispatch(new NestedAction());
                _service.AssertSingle();
            }
        }
        private class NestedAction : IMediatorAction
        {

        }
        private class NestedActionHandler : IMediatorHandler<NestedAction>
        {
            private readonly FakeService _service;

            public NestedActionHandler(FakeService service)
            {
                _service = service;
            }

            public Task Handle(NestedAction action, CancellationToken cancellationToken)
            {
                _service.AssertTwo();
                return Task.CompletedTask;
            }
        }
    }


}
