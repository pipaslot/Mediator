using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests.Services;

public class ServiceProviderExtensionsTests
{
    [Test]
    public async Task GetRequestHandlers_BindingTypeclassForDirectClassName_ReturnHandler()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.RequestHandler>();
        var handlers = services.GetRequestHandlers(
            typeof(SingleHandler.Request),
            typeof(SingleHandler.Response));
        await Assert.That(handlers).HasSingleItem();
    }
}