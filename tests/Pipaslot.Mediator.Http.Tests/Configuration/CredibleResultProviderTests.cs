using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Configuration;

public class CredibleResultProviderTests
{
    [Theory]
    [InlineData(typeof(CustomResult))]
    [InlineData(typeof(CustomResult[]))]
    [InlineData(typeof(List<CustomResult>))]
    public void VerifyCredibility_RegisteredCustomResultType_Pass(Type tested)
    {
        var sut = Create(c => { }, typeof(CustomResult));
        sut.VerifyCredibility(tested);
    }

    [Theory]
    [InlineData(typeof(CustomResult))]
    [InlineData(typeof(CustomResult[]))]
    [InlineData(typeof(List<CustomResult>))]
    public void VerifyCredibility_RegisteredCustomResultTypeAssembly_Pass(Type tested)
    {
        var sut = CreateForAssembly(c => { }, typeof(CustomResult).Assembly);
        sut.VerifyCredibility(tested);
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

    [Theory]
    [InlineData(typeof(Result))]
    [InlineData(typeof(Result[]))]
    [InlineData(typeof(List<Result>))]
    public void VerifyCredibility_RegisteredActionResultType_Pass(Type testedType)
    {
        var sut = Create(c => c.AddActionsFromAssemblyOf<FakeRequest>());
        sut.VerifyCredibility(testedType);
    }

    private CredibleResultProvider Create(Action<MediatorConfigurator> setup, params Type[] customTypes)
    {
        var serviceCollectionMock = new Mock<IServiceCollection>();
        var configurator = new MediatorConfigurator(serviceCollectionMock.Object);
        setup(configurator);
        return new CredibleResultProvider(configurator, customTypes, Array.Empty<Assembly>());
    }

    private CredibleResultProvider CreateForAssembly(Action<MediatorConfigurator> setup, params Assembly[] customTypes)
    {
        var serviceCollectionMock = new Mock<IServiceCollection>();
        var configurator = new MediatorConfigurator(serviceCollectionMock.Object);
        setup(configurator);
        return new CredibleResultProvider(configurator, new Type[0], customTypes);
    }

    private class CustomResult;

    private class FakeRequest : IRequest<Result[]>;

    private class Result;
}