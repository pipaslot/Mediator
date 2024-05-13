using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

public class ContextFlowTests
{
    private readonly ContextFlow _flow = new();

    [Fact]
    public void GetCurrent_ReturnsTheLast()
    {
        _flow.Add(CreateContext(1));
        _flow.Add(CreateContext(2));
        AssertDepth(2, _flow.GetCurrent());
    }
    
    [Fact]
    public void ToArray_FirstIsTheCurrent()
    {
        _flow.Add(CreateContext(1));
        _flow.Add(CreateContext(2));
        AssertDepth(2, _flow.ToArray().First());
    }
    
    [Fact]
    public void ToArray_LastIsTheRootOne()
    {
        _flow.Add(CreateContext(1));
        _flow.Add(CreateContext(2));
        AssertDepth(1, _flow.ToArray().Last());
    }
    
    [Fact]
    public async Task Flow_AddInSequence()
    {
        AssertCount(0);
        await TriggerAction(1, AssertCountFunc(1));
        AssertCount(0);
        await TriggerAction(1, AssertCountFunc(1));
        AssertCount(0);
    }
    
    /// <summary>
    /// Simulate use case when we trigger another action from already processed action
    /// </summary>
    [Fact]
    public async Task Flow_AddInSequenceNested()
    {
        AssertCount(0);
        await TriggerAction(1, async () =>
        {
            AssertCount(1);
            await TriggerAction(2, AssertCountFunc(2));
            AssertCount(1);
            await TriggerAction(2, AssertCountFunc(2));
            AssertCount(1);
        });
        AssertCount(0);
        await TriggerAction(1, async () =>
        {
            AssertCount(1);
            await TriggerAction(2, AssertCountFunc(2));
            AssertCount(1);
            await TriggerAction(2, AssertCountFunc(2));
            AssertCount(1);
        });
        AssertCount(0);
    }
    
    [Fact]
    public async Task Flow_AddInParallel()
    {
        AssertCount(0);
        var actions = Enumerable.Range(1, 2)
            .Select(i => TriggerAction(1, AssertCountFunc(1)))
            .ToArray();
        await Task.WhenAll(actions);
        AssertCount(0);
    }
    
    /// <summary>
    /// Simulate use case when we trigger another action from already processed action (in paralle/concurrently)
    /// </summary>
    [Fact]
    public async Task Flow_AddInParallelNested()
    {
        AssertCount(0);
        var actions = Enumerable.Range(1, 2)
            .Select(i => TriggerAction(1,async () =>
            {
                AssertCount(1);
                await TriggerAction(2, AssertCountFunc(2));
                AssertCount(1);
                await TriggerAction(2, AssertCountFunc(2));
                AssertCount(1);
            }))
            .ToArray();
        await Task.WhenAll(actions);
        AssertCount(0);
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
        return ()=>
        {
            AssertCount(expected);
            return Task.CompletedTask;
        };
    }

    private void AssertCount(int expected)
    {
        var asArray = _flow.ToArray();
        Assert.Equal(expected, asArray.Count());

        var expectedRange = Enumerable
            .Range(1, expected)
            .ToArray();
        var actual = asArray
            .Select(context => ((FakeAction)context.Action).Depth)
            .Reverse()
            .ToArray();
        Assert.Equal(expectedRange, actual);
    }

    private void AssertDepth(int expected, MediatorContext? context)
    {
        var actual = (context?.Action as FakeAction)?.Depth ?? -1;
        Assert.Equal(expected, actual);
    }


    private MediatorContext CreateContext(int depth)
    {
        var mediatorMock = new Mock<IMediator>();
        var mediatorContextAccessorMock = new Mock<IMediatorContextAccessor>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var action = new FakeAction(depth);
        return new MediatorContext(mediatorMock.Object, mediatorContextAccessorMock.Object, serviceProviderMock.Object, action,
            CancellationToken.None, null, null);
    }

    private record FakeAction(int Depth) : IMediatorAction;
}