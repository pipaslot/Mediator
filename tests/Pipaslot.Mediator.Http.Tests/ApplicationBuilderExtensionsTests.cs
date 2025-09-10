using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Services;
using System;

namespace Pipaslot.Mediator.Http.Tests;
public class ApplicationBuilderExtensionsTests
{
    private static readonly Exception _exception = new();
    [Test]
    public void UseMediator_CheckMatchingHandlersEnabled_ResolveAndExecuteHandlerExistenceChecker()
    {
        var ApplicationBuilder = CreateApplicationBuilder();
        var expectedEx = Assert.Throws<Exception>(() => ApplicationBuilder.UseMediator(true));
        Assert.Equal(_exception, expectedEx);
    }

    [Test]
    public void UseMediator_CheckMatchingHandlersDisabled_ServiceResolverIsNotExecuted()
    {
        var ApplicationBuilder = CreateApplicationBuilder();
        ApplicationBuilder.UseMediator();
    }

    private IApplicationBuilder CreateApplicationBuilder()
    {
        var servicesMock = new Mock<IServiceCollection>();
        var serviceProvider = new ServiceCollection().AddSingleton<IHandlerExistenceChecker>(_ => new FakeChecker()).AddSingleton(_ => new MediatorConfigurator(servicesMock.Object)).BuildServiceProvider();
        var ApplicationBuilderMock = new Mock<IApplicationBuilder>();
        ApplicationBuilderMock.Setup(x => x.ApplicationServices).Returns(serviceProvider);
        return ApplicationBuilderMock.Object;
    }

    private class FakeChecker : Pipaslot.Mediator.Services.IHandlerExistenceChecker
    {
        public void Verify(ExistenceCheckerSetting setting)
        {
            if (setting.CheckMatchingHandlers)
            {
                throw _exception;
            }
        }
    }
}