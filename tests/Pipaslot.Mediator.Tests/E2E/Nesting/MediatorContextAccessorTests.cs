using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.Nesting;

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
            .UseActionEvents();
        collection.AddScoped<FakeService>();
        collection.AddTransient<IMediatorHandler<Level1Action>, Level1ActionHandler>();
        collection.AddTransient<IMediatorHandler<Level2Action>, Level2ActionHandler>();
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

    private class FakeService(IMediatorContextAccessor accessor)
    {
        public void AssertZero()
        {
            Assert.Empty(accessor.ContextStack);
            Assert.Null(accessor.Context);
            // Verify that helper classes returns the same result as well
            Assert.Null(accessor.GetRootContext());
            Assert.Empty(accessor.GetParentContexts());
        }

        public void AssertSingle()
        {
            Assert.Equal(typeof(Level1Action), accessor.Context?.Action?.GetType());
            Assert.Single(accessor.ContextStack);
            // Verify that helper classes returns the same result as well
            Assert.Equal(typeof(Level1Action), accessor.GetRootContext()?.Action?.GetType());
            Assert.Empty(accessor.GetParentContexts());
            // Root execution: depth 1, not nested
            Assert.Equal(1, accessor.Context?.Depth);
            Assert.False(accessor.Context?.IsNested);
        }

        public void AssertTwo()
        {
            Assert.Equal(typeof(Level2Action), accessor.Context?.Action.GetType());
            Assert.Equal(2, accessor.ContextStack.Count);
            // Verify that helper classes returns the same result as well
            Assert.Equal(typeof(Level1Action), accessor.GetRootContext()?.Action.GetType());
            Assert.Single(accessor.GetParentContexts());
            Assert.Equal(typeof(Level1Action), accessor.GetParentContexts().First().Action.GetType());
            // Nested execution: depth 2, nested; parent remains at depth 1
            Assert.Equal(2, accessor.Context?.Depth);
            Assert.True(accessor.Context?.IsNested);
            Assert.Equal(1, accessor.GetParentContexts().First().Depth);
            Assert.False(accessor.GetParentContexts().First().IsNested);
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
            service.AssertSingle();
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

            service.AssertSingle();
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
            service.AssertTwo();
            if (action.Delay.HasValue)
            {
                await Task.Delay(action.Delay.Value, cancellationToken);
            }

            service.AssertTwo();
        }
    }
}