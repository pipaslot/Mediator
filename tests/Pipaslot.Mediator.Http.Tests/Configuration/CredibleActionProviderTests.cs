using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http;
using Pipaslot.Mediator.Http.Configuration;
using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Configuration
{
    public class CredibleActionProviderTests
    {
        [Fact]
        public void VerifyCredibility_ContractTypeIsNotFromRegisteredAssembly_ThrowException()
        {
            var sut = Create(c => c.AddActionsFromAssemblyOf<Mediator>());
            var exception = Assert.Throws<MediatorHttpException>(() =>
            {
                sut.VerifyCredibility(typeof(FakeContract));
            });
            Assert.Equal(MediatorHttpException.CreateForUnregisteredActionType(typeof(FakeContract)).Message, exception.Message);
        }

        [Fact]
        public void VerifyCredibility_RegisteredContractTypeNotImplementingIActionMarkerInterface_ThrowException()
        {
            var sut = Create(c => c.AddActionsFromAssemblyOf<FakeNonContract>());
            var exception = Assert.Throws<MediatorHttpException>(() =>
            {
                sut.VerifyCredibility(typeof(FakeNonContract));
            });
            Assert.Equal(MediatorHttpException.CreateForNonContractType(typeof(FakeNonContract)).Message, exception.Message);
        }

        [Fact]
        public void VerifyCredibility_ContractTypeIsFromRegisteredAssembly_Pass()
        {
            var sut = Create(c => c.AddActionsFromAssemblyOf<FakeContract>());
            sut.VerifyCredibility(typeof(FakeContract));
        }

        private CredibleActionProvider Create(Action<PipelineConfigurator> setup)
        {
            var serviceCollctionMock = new Mock<IServiceCollection>();
            var configurator = new PipelineConfigurator(serviceCollctionMock.Object);
            setup(configurator);
            return new CredibleActionProvider(configurator);
        }

        public class FakeContract : IMessage
        {

        }

        public class FakeNonContract
        {

        }
    }
}
