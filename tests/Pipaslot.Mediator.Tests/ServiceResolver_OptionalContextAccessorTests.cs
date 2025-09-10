using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests;

public class ServiceResolver_OptionalContextAccessorTests
{
    [Test]
    public async Task WithoutContextAccessor_MediatorCallPasses()
    {
        var mediator = CreateMediator(false);
        var result = await mediator.Dispatch(new NopMessage());
        Assert.True(result.Success);
    }
    [Test]
    public async Task WithContextAccessor_MediatorCallPasses()
    {
        var mediator = CreateMediator(true);
        var result = await mediator.Dispatch(new NopMessage());
        Assert.True(result.Success);
    }

    private static IMediator CreateMediator(bool addContextAccessor)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddMediator(addContextAccessor: addContextAccessor)
            .AddHandlers([typeof(NopMesageHandler)]);
        var sp = collection.BuildServiceProvider();
        return sp.GetRequiredService<IMediator>();
    }
}