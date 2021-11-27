using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Http;
using Pipaslot.Mediator.Http.Contracts;
using System;
using Xunit;

namespace Pipaslot.Mediator.Tests.Http
{
    public class MediatorMiddleware_WhitelistDeserializedTypesTests
    {
        [Fact]
        public void ExecuteQuery_ContractTypeIsNotFromRegisteredAssembly_ThrowException()
        {
            var sut = CreateConfigurator(c => { });
            var contractType = typeof(FakeContract);
            var request = CreateRequest(contractType);
            var exception = Assert.Throws<MediatorHttpException>(() =>
            {
                sut.Validate(request);
            });
            Assert.Equal(MediatorHttpException.CreateForUnregisteredType(contractType).Message, exception.Message);
        }

        [Fact]
        public void ExecuteQuery_RegisteredContractTypeNotImplementingIActionMarkerInterface_ThrowException()
        {
            var sut = CreateConfigurator(c => c.AddActionsFromAssemblyOf<FakeNonContract>());
            var contractType = typeof(FakeNonContract);
            var request = CreateRequest(contractType);
            var exception = Assert.Throws<MediatorHttpException>(() =>
            {
                sut.Validate(request);
            });
            Assert.Equal(MediatorHttpException.CreateForNonContractType(contractType).Message, exception.Message);
        }

        [Fact]
        public void ExecuteQuery_ContractTypeIsFromRegisteredAssembly_Pass()
        {
            var sut = CreateConfigurator(c => c.AddActionsFromAssemblyOf<FakeContract>());

            var request = CreateRequest(typeof(FakeContract));
            sut.Validate(request);
        }

        private static MediatorMiddleware CreateConfigurator(Action<PipelineConfigurator> setup)
        {
            var mediatorMock = new Mock<IMediator>();
            var serviceCollctionMock = new Mock<IServiceCollection>();
            var configurator = new PipelineConfigurator(serviceCollctionMock.Object);
            setup(configurator);
            return new MediatorMiddleware(null, null, new ContractSerializer(), configurator);
        }

        private static MediatorRequestDeserialized CreateRequest(Type objectType)
        {
            var content = Activator.CreateInstance(objectType);
            return new MediatorRequestDeserialized(content, objectType, objectType.AssemblyQualifiedName);
        }

        public class FakeContract : IMessage
        {

        }

        public class FakeNonContract
        {

        }

    }
}
