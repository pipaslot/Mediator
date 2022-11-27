using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Services;
using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests
{
    public class ApplicationBuilderExtensionsTests
    {
        private Exception _exception = new Exception();

        [Fact]
        public void UseMediator_CheckMatchingHandlersEnabled_ResolveAndExecuteHandlerExistenceChecker()
        {
            var ApplicationBuilder = CreateApplicationBuilder();

            var expectedEx = Assert.Throws<Exception>(() => ApplicationBuilderExtensions.UseMediator(ApplicationBuilder, true));
            Assert.Equal(_exception, expectedEx);
        }

        [Fact]
        public void UseMediator_CheckMatchingHandlersDisabled_ServiceResolverIsNotExecuted()
        {
            var ApplicationBuilder = CreateApplicationBuilder();

            ApplicationBuilderExtensions.UseMediator(ApplicationBuilder, false);
        }

        private IApplicationBuilder CreateApplicationBuilder()
        {
            var handlerExistenceCheckerMock = new Mock<IHandlerExistenceChecker>();
            handlerExistenceCheckerMock
                .Setup(x => x.Verify(true, false))
                .Throws(_exception);

            var serviceProvider = new ServiceCollection()
                .AddSingleton(_ => handlerExistenceCheckerMock.Object)
                .BuildServiceProvider();

            var ApplicationBuilderMock = new Mock<IApplicationBuilder>();
            ApplicationBuilderMock
                .Setup(x => x.ApplicationServices)
                .Returns(serviceProvider);

            return ApplicationBuilderMock.Object;
        }
    }
}
