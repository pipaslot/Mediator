using Pipaslot.Mediator.Tests.FakeActions;
using Pipaslot.Mediator.Services;
using System;
using Xunit;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Tests.Services
{
    public class ServiceProviderExtensionsTests
    {
        [Fact]
        public void GetRequestHandlers_BindingTypeInterfaceForIMediatorAction_ReturnHandler()
        {
            RunGetRequestHandlers<SingleInterfaceHandler.RequestInterfaceForMediatorActionHandler>(
                typeof(SingleInterfaceHandler.RequestInterfaceForMediatorAction),
                ActionToHandlerBindingType.Interface);
        }

        [Fact]
        public void GetRequestHandlers_BindingTypeInterfaceForInterfaceInheritingMediatorAction_ReturnHandler()
        {
            RunGetRequestHandlers<SingleInterfaceHandler.RequestInterfaceForMediatorActionHandler>(
                typeof(SingleInterfaceHandler.RequestInterfaceForMediatorAction),
                ActionToHandlerBindingType.Interface);
        }

        [Fact]
        public void GetRequestHandlers_BindingTypeclassForDirectClassName_ReturnHandler()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.RequestHandler>();
            var handlers = services.GetRequestHandlers(
                typeof(SingleHandler.Request),
                typeof(SingleHandler.Response),
                ActionToHandlerBindingType.Class);
            Assert.Single(handlers);
        }

        //RequestInterfaceForMediatorActionHandler

        private void RunGetRequestHandlers<THandler>(Type requestType, ActionToHandlerBindingType binding)
        {
            var services = Factory.CreateServiceProviderWithHandlers<THandler>();
            var handlers = services.GetRequestHandlers(
                requestType,
                typeof(SingleInterfaceHandler.Response),
                binding);
            Assert.Single(handlers);
        }
    }
}
