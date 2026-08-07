using Microsoft.Extensions.DependencyInjection;

namespace Pipaslot.Mediator.Http.Tests.Configuration;

public class ClientMediatorOptions_ContextAccessorTests
{
    [Fact]
    public void AddMediatorClient_WithoutConfiguration_ContextAccessorNotRegistered()
    {
        var collection = new ServiceCollection();
        collection.AddMediatorClient();
        var services = collection.BuildServiceProvider();

        Assert.Null(services.GetService<IMediatorContextAccessor>());
    }

    [Fact]
    public void AddMediatorClient_WithContextAccessorEnabled_ContextAccessorRegistered()
    {
        var collection = new ServiceCollection();
        collection.AddMediatorClient(o => o.AddContextAccessor = true);
        var services = collection.BuildServiceProvider();

        Assert.NotNull(services.GetService<IMediatorContextAccessor>());
    }

    [Fact]
    public void AddMediatorServer_WithoutConfiguration_ContextAccessorRegistered()
    {
        var collection = new ServiceCollection();
        collection.AddMediatorServer();
        var services = collection.BuildServiceProvider();

        Assert.NotNull(services.GetService<IMediatorContextAccessor>());
    }
}
