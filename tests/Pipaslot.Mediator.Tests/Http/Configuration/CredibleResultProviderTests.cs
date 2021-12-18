using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http;
using Pipaslot.Mediator.Http.Configuration;
using System;
using Xunit;

namespace Pipaslot.Mediator.Tests.Http.Configuration
{
    public class CredibleResultProviderTests
    {
        [Fact]
        public void VerifyCredibility_RegisteredCustomResultType_Pass()
        {
            var sut = Create(c => { }, typeof(CustomResult));
            sut.VerifyCredibility(typeof(CustomResult));
        }

        [Fact]
        public void VerifyCredibility_NonRegisteredCustomResultType_ThrowException()
        {
            var sut = Create(c => { });
            var exception = Assert.Throws<MediatorHttpException>(() =>
            {
                sut.VerifyCredibility(typeof(CustomResult));
            });
            Assert.Equal(MediatorHttpException.CreateForUnregisteredResultType(typeof(CustomResult)).Message, exception.Message);
        }

        [Fact]
        public void VerifyCredibility_NonRegisteredActionResultType_ThrowException()
        {
            var sut = Create(c => { });
            var exception = Assert.Throws<MediatorHttpException>(() =>
            {
                sut.VerifyCredibility(typeof(Result));
            });
            Assert.Equal(MediatorHttpException.CreateForUnregisteredResultType(typeof(Result)).Message, exception.Message);
        }

        [Fact]
        public void VerifyCredibility_RegisteredActionResultType_Pass()
        {
            var sut = Create(c => c.AddActionsFromAssemblyOf<FakeRequest>());
            sut.VerifyCredibility(typeof(Result));
        }

        private CredibleResultProvider Create(Action<PipelineConfigurator> setup, params Type[] customTypes)
        {
            var serviceCollectionMock = new Mock<IServiceCollection>();
            var configurator = new PipelineConfigurator(serviceCollectionMock.Object);
            setup(configurator);
            return new CredibleResultProvider(configurator, customTypes);
        }

        private class CustomResult
        {

        }
        private class FakeRequest : IRequest<Result>
        {

        }
        private class Result
        {

        }
    }
}
