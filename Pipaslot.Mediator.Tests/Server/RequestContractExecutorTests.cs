using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Contracts;
using Pipaslot.Mediator.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests.Server
{
    public class RequestContractExecutorTests
    {
        [Fact]
        public async Task ExecuteQuery_ContractTypeIsNotFromRegisteredAssembly_ThrowException()
        {
            var sut = CreateConfigurator(c => { });
            var contractType = typeof(FakeContract);
            var request = CreateRequest(contractType);
            var exception = await Assert.ThrowsAsync<MediatorServerException>(async () =>
            {
                await sut.ExecuteQuery(request, System.Threading.CancellationToken.None);
            });
            Assert.Equal(MediatorServerException.CreateForUnregisteredType(contractType).Message, exception.Message);
        }

        // TODO Consider implementing this check in next version
        //[Fact]
        //public async Task ExecuteQuery_RegisteredContractTypeNotImplementingIActionMarkerInterface_ThrowException()
        //{
        //    var sut = CreateConfigurator(c => c.AddActionsFromAssemblyOf<FakeNonContract>());
        //    var contractType = typeof(FakeNonContract);
        //    var request = CreateRequest(contractType);
        //    var exception = await Assert.ThrowsAsync<MediatorServerException>(async () =>
        //    {
        //        await sut.ExecuteQuery(request, System.Threading.CancellationToken.None);
        //    });
        //    Assert.Equal(MediatorServerException.CreateForNonContractType(contractType).Message, exception.Message);
        //}

        [Fact]
        public async Task ExecuteQuery_ContractTypeIsFromRegisteredAssembly_Pass()
        {
            var sut = CreateConfigurator(c => c.AddActionsFromAssemblyOf<FakeContract>());

            var request = CreateRequest(typeof(FakeContract));
            await sut.ExecuteQuery(request, System.Threading.CancellationToken.None);
        }

        private RequestContractExecutor CreateConfigurator(Action<PipelineConfigurator> setup)
        {
            var mediatorMock = new Mock<IMediator>();
            var serviceCollctionMock = new Mock<IServiceCollection>();
            var configurator = new PipelineConfigurator(serviceCollctionMock.Object);
            setup(configurator);
            return new RequestContractExecutor(mediatorMock.Object, configurator, null);
        }

        private MediatorRequestSerializable CreateRequest(Type objectType)
        {
            return new MediatorRequestSerializable()
            {
                ObjectName = objectType.AssemblyQualifiedName,
                Json = "{}"
            };
        }

        public class FakeContract : IMessage
        {

        }

        public class FakeNonContract
        {

        }

    }
}
