﻿using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests.Services;

public class ServiceProviderExtensionsTests
{
    [Fact]
    public void GetRequestHandlers_BindingTypeclassForDirectClassName_ReturnHandler()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.RequestHandler>();
        var handlers = services.GetRequestHandlers(
            typeof(SingleHandler.Request),
            typeof(SingleHandler.Response));
        Assert.Single(handlers);
    }
}