using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Services;
using System;

namespace Pipaslot.Mediator.Http.Tests;

public class ApplicationBuilderExtensionsTests
{
    private static readonly Exception _exception = new();

    [Fact]
    public void UseMediator_CheckMatchingHandlersEnabled_ResolveAndExecuteHandlerExistenceChecker()
    {
        var ApplicationBuilder = CreateApplicationBuilder();

        var expectedEx = Assert.Throws<Exception>(() => ApplicationBuilder.UseMediator(true));
        Assert.Equal(_exception, expectedEx);
    }

    [Fact]
    public void UseMediator_CheckMatchingHandlersDisabled_ServiceResolverIsNotExecuted()
    {
        var ApplicationBuilder = CreateApplicationBuilder();

        ApplicationBuilder.UseMediator();
    }

    private IApplicationBuilder CreateApplicationBuilder()
    {
        var services = Substitute.For<IServiceCollection>();
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IHandlerExistenceChecker>(_ => new FakeChecker())
            .AddSingleton(_ => new MediatorConfigurator(services))
            .BuildServiceProvider();

        var applicationBuilder = Substitute.For<IApplicationBuilder>();
        applicationBuilder.ApplicationServices.Returns(serviceProvider);

        return applicationBuilder;
    }

    private class FakeChecker : IHandlerExistenceChecker
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