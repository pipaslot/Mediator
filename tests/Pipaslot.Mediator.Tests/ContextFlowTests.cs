using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Linq;
using System.Threading;

namespace Pipaslot.Mediator.Tests;

public class ContextFlowTests
{
    private readonly ContextFlow _flow = new();

    [Test]
    public async Task GetCurrent_ReturnsTheLast()
    {
        _flow.Add(CreateContext(1));
        _flow.Add(CreateContext(2));
        await AssertDepth(2, _flow.GetCurrent());
    }

    [Test]
    public async Task ToArray_FirstIsTheCurrent()
    {
        _flow.Add(CreateContext(1));
        _flow.Add(CreateContext(2));
        await AssertDepth(2, _flow.ToArray().First());
    }

    [Test]
    public async Task ToArray_LastIsTheRootOne()
    {
        _flow.Add(CreateContext(1));
        _flow.Add(CreateContext(2));
        await AssertDepth(1, _flow.ToArray().Last());
    }

    [Test]
    public async Task Flow_AddInSequence()
    {
        await AssertCount(0);
        await TriggerAction(1, AssertCountFunc(1));
        await AssertCount(0);
        await TriggerAction(1, AssertCountFunc(1));
        await AssertCount(0);
    }

    /// <summary>
    /// Simulate use case when we trigger another action from already processed action
    /// </summary>
    [Test]
    public async Task Flow_AddInSequenceNested()
    {
        await AssertCount(0);
        await TriggerAction(1, async () =>
        {
            await AssertCount(1);
            await TriggerAction(2, AssertCountFunc(2));
            await AssertCount(1);
            await TriggerAction(2, AssertCountFunc(2));
            await AssertCount(1);
        });
        await AssertCount(0);
        await TriggerAction(1, async () =>
        {
            await AssertCount(1);
            await TriggerAction(2, AssertCountFunc(2));
            await AssertCount(1);
            await TriggerAction(2, AssertCountFunc(2));
            await AssertCount(1);
        });
        await AssertCount(0);
    }

    [Test]
    public async Task Flow_AddInParallel()
    {
        await AssertCount(0);
        var actions = Enumerable.Range(1, 2)
            .Select(i => TriggerAction(1, AssertCountFunc(1)))
            .ToArray();
        await Task.WhenAll(actions);
        await AssertCount(0);
    }

    /// <summary>
    /// Simulate use case when we trigger another action from already processed action (in paralle/concurrently)
    /// </summary>
    [Test]
    public async Task Flow_AddInParallelNested()
    {
        await AssertCount(0);
        var actions = Enumerable.Range(1, 2)
            .Select(i => TriggerAction(1, async () =>
            {
                await AssertCount(1);
                await TriggerAction(2, AssertCountFunc(2));
                await AssertCount(1);
                await TriggerAction(2, AssertCountFunc(2));
                await AssertCount(1);
            }))
            .ToArray();
        await Task.WhenAll(actions);
        await AssertCount(0);
    }

    /// <summary>
    /// Prevent System.InvalidOperationException: Collection was modified after the enumerator was instantiated.
    /// </summary>
    [Test]
    public async Task ConcurrencyAccess_ShouldNotFail()
    {
        var actions = Enumerable.Range(1, 1000)
            .Select(i => TriggerAction(1, async () =>
            {
                await Task.Yield(); // force asynchronous context switch
                await TriggerAction(2, async () =>
                {
                    var current = _flow.GetCurrent();
                    await Assert.That(current).IsNotNull();
                });
                await TriggerAction(2, async () =>
                {
                    var current = _flow.GetCurrent();
                    await Assert.That(current).IsNotNull();
                });
            }))
            .ToArray();
        await Task.WhenAll(actions);
    }

    /// <summary>
    /// Simulate action execution with handler
    /// </summary>
    private async Task TriggerAction(int depth, Func<Task> nestedAction)
    {
        _flow.Add(CreateContext(depth));
        await nestedAction.Invoke();
        await Task.Delay(5); //Fake action doing operation and causing continuation in different thread
    }

    private Func<Task> AssertCountFunc(int expected)
    {
        return async () =>
        {
            await AssertCount(expected);
        };
    }

    private async Task AssertCount(int expected)
    {
        var asArray = _flow.ToArray();
        await Assert.That(asArray.Count()).IsEqualTo(expected);

        var expectedRange = Enumerable
            .Range(1, expected)
            .ToArray();
        var actual = asArray
            .Select(context => ((FakeAction)context.Action).Depth)
            .Reverse()
            .ToArray();
        await Assert.That(actual).IsEqualTo(expectedRange);
    }

    private async Task AssertDepth(int expected, MediatorContext? context)
    {
        var actual = (context?.Action as FakeAction)?.Depth ?? -1;
        await Assert.That(actual).IsEqualTo(expected);
    }


    private MediatorContext CreateContext(int depth)
    {
        var mediatorMock = new Mock<IMediator>();
        var mediatorContextAccessorMock = new Mock<IMediatorContextAccessor>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var action = new FakeAction(depth);
        return new MediatorContext(mediatorMock.Object, mediatorContextAccessorMock.Object, serviceProviderMock.Object, new ReflectionCache(), action,
            CancellationToken.None, null, null);
    }

    private record FakeAction(int Depth) : IMediatorAction;
}