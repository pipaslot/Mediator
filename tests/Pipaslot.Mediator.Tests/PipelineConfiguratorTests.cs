using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class PipelineConfiguratorTests
    {
        [Fact]
        public void AddActions_NonActionTypePassed_ThrowException()
        {
            var sut = Create();
            Assert.Throws<MediatorException>(() =>
            {
                sut.AddActions(new Type[] { typeof(object) });
            });
        }

        [Fact]
        public void AddActions_ActionTypePassed_Pass()
        {
            var sut = Create();
            sut.AddActions(new Type[] { typeof(NopMessage) });
        }

        [Fact]
        public void AddHandlers_NonHandlerTypePassed_ThrowException()
        {
            var sut = Create();
            Assert.Throws<MediatorException>(() =>
            {
                sut.AddHandlers(new Type[] { typeof(object) });
            });
        }

        [Fact]
        public void AddHandlers_HandlerTypePassed_Pass()
        {
            var sut = Create();
            sut.AddHandlers(new Type[] { typeof(NopMesageHandler) });
        }

        private PipelineConfigurator Create()
        {
            var sc = new Mock<IServiceCollection>();
            return new PipelineConfigurator(sc.Object);
        }
    }
}
