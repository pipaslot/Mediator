using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Services;
using Pipaslot.Mediator.Tests.InvalidActions;
using Pipaslot.Mediator.Tests.ValidActions;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Tests.Services
{
    public class HandlerExistenceCheckerTests
    {
        [Fact]
        public void Verify_RegisteredAssemblyWithValidActions_DoesNotThrowExceptions()
        {
            var sp = Factory.CreateServiceProvider(c =>
            {
                c.AddActionsFromAssemblyOf<NopMessage>();
                c.AddHandlersFromAssemblyOf<NopMesageHandler>();
            });
            var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
            sut.Verify();
        }

        [Fact]
        public void Verify_MessageWithoutHandler_ThrowExceptions()
        {
            ShouldThrow(MediatorException.CreateForNoHandler(typeof(MessageWithoutHandler)).Message);
        }

        [Fact]
        public void Verify_RequestWithoutHandler_ThrowExceptions()
        {
            ShouldThrow(MediatorException.CreateForNoHandler(typeof(RequestWithoutHandler)).Message);
        }

        private void ShouldThrow(string expectedError)
        {
            var sp = Factory.CreateServiceProvider(c =>
            {
                c.AddActionsFromAssemblyOf<MessageWithoutHandler>();
                c.AddHandlersFromAssemblyOf<MessageWithoutHandler>();
            });
            var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
            var ex = Assert.Throws<MediatorException>(() =>
            {
                sut.Verify();
            });

            Assert.Contains(expectedError, ex.Message);
        }
    }
}
