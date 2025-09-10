using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;
using System.Threading;

namespace Pipaslot.Mediator.Tests;

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
            .AddActionsFromAssembly(Factory.Assembly)
            .UseActionEvents();
        collection.AddScoped<FakeService>();
        collection.AddTransient<IMediatorHandler<Level1Action>, Level1ActionHandler>();
        collection.AddTransient<IMediatorHandler<Level2Action>, Level2ActionHandler>();
        var services = collection.BuildServiceProvider();

        _mediator = services.GetRequiredService<IMediator>();
        _contextAccessor = services.GetRequiredService<IMediatorContextAccessor>();
        _service = services.GetRequiredService<FakeService>();
    }

    [Test]
    public async Task NoAction_ContextIsNull()
    {
        await Assert.That(_contextAccessor.Context).IsNull();
        await Assert.That(_contextAccessor.ContextStack).IsEmpty();
    }

    [Test]
    public async Task ExecutionCompleted_ContextIsNull()
    {
        await _mediator.DispatchUnhandled(new Level1Action(ActionBehaviorTestCase.SingleNested));
        await Assert.That(_contextAccessor.Context).IsNull();
        await Assert.That(_contextAccessor.ContextStack).IsEmpty();
    }

    [Test]
    public async Task Flow()
    {
        await _service.AssertZero();
        await _mediator.DispatchUnhandled(new Level1Action(ActionBehaviorTestCase.SingleNested));
        await _service.AssertZero();
    }

    [Test]
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

        public async Task AssertZero()
        {
            await Assert.That(_accessor.ContextStack).IsEmpty();
            await Assert.That(_accessor.Context).IsNull();
            // Verify that helper classes returns the same result as well
            await Assert.That(_accessor.GetRootContext()).IsNull();
            await Assert.That(_accessor.GetParentContexts()).IsEmpty();
        }

        public async Task AssertSingle()
        {
            await Assert.That(_accessor.Context?.Action?.GetType()).IsEqualTo(typeof(Level1Action));
            await Assert.That(_accessor.ContextStack.Count).IsEqualTo(1);
            // Verify that helper classes returns the same result as well
            await Assert.That(_accessor.GetRootContext()?.Action?.GetType()).IsEqualTo(typeof(Level1Action));
            await Assert.That(_accessor.GetParentContexts()).IsEmpty();
        }

        public async Task AssertTwo()
        {
            await Assert.That(_accessor.Context?.Action.GetType()).IsEqualTo(typeof(Level2Action));
            await Assert.That(_accessor.ContextStack.Count).IsEqualTo(2);
            // Verify that helper classes returns the same result as well
            await Assert.That(_accessor.GetRootContext()?.Action.GetType()).IsEqualTo(typeof(Level1Action));
            await Assert.That(_accessor.GetParentContexts().Count()).IsEqualTo(1);
            await Assert.That(_accessor.GetParentContexts().First().Action.GetType()).IsEqualTo(typeof(Level1Action));
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

    private class Level1ActionHandler(FakeService service, IMediator mediator) : IMediatorHandler<Level1Action>
    {
        public async Task Handle(Level1Action action, CancellationToken cancellationToken)
        {
            await service.AssertSingle();
            if (action.Case == ActionBehaviorTestCase.SingleNested)
            {
                await mediator.DispatchUnhandled(new Level2Action(TimeSpan.FromMilliseconds(10)), cancellationToken);
            }
            else if (action.Case == ActionBehaviorTestCase.ConcurrentNested)
            {
                var actions = new[]
                {
                    new Level2Action(TimeSpan.FromMilliseconds(50)), new Level2Action(TimeSpan.FromMilliseconds(20)),
                    new Level2Action(TimeSpan.FromMilliseconds(10))
                };
                var tasks = actions.Select(async a => await mediator.DispatchUnhandled(a, cancellationToken));
                await Task.WhenAll(tasks);
            }
            else
            {
                throw new NotImplementedException();
            }

            await service.AssertSingle();
        }
    }

    /// <summary>
    /// Action executed by Level 1
    /// </summary>
    /// <param name="Delay"></param>
    private record Level2Action(TimeSpan? Delay = null) : IMediatorAction;

    private class Level2ActionHandler(FakeService service) : IMediatorHandler<Level2Action>
    {
        public async Task Handle(Level2Action action, CancellationToken cancellationToken)
        {
            await service.AssertTwo();
            if (action.Delay.HasValue)
            {
                await Task.Delay(action.Delay.Value, cancellationToken);
            }

            await service.AssertTwo();
        }
    }
}