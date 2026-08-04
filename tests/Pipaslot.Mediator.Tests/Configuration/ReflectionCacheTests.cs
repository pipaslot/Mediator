using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares.Handlers;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Tests.Configuration;

/// <summary>
/// <see cref="ReflectionCache"/> is constructed by every context built through <see cref="MediatorContext.Create"/>,
/// but always as an incidental dependency of <see cref="MediatorContext"/> — none of those tests exercise it as the SUT or
/// call <see cref="ReflectionCache.AddActions"/> first, so the startup-time lookup path
/// (<c>_startupTimeActions</c>) and the lazy runtime-resolution path (<c>_runtimeActions</c>) it falls back to for
/// an action never registered up front were both unverified.
/// </summary>
public class ReflectionCacheTests
{
    [Fact]
    public void GetHandlerExecutorType_MessageRegisteredViaAddActions_ReturnsMessageHandlerExecutorType()
    {
        var sut = new ReflectionCache().AddActions(typeof(SampleMessage));

        var result = sut.GetHandlerExecutorType(typeof(SampleMessage));

        Assert.Equal(typeof(MessageHandlerExecutor<SampleMessage>), result);
    }

    [Fact]
    public void GetHandlerExecutorType_RequestRegisteredViaAddActions_ReturnsRequestHandlerExecutorType()
    {
        var sut = new ReflectionCache().AddActions(typeof(SampleRequest));

        var result = sut.GetHandlerExecutorType(typeof(SampleRequest));

        Assert.Equal(typeof(RequestHandlerExecutor<SampleRequest, string>), result);
    }

    [Fact]
    public void GetRequestResultType_MessageRegisteredViaAddActions_ReturnsNull()
    {
        var sut = new ReflectionCache().AddActions(typeof(SampleMessage));

        var result = sut.GetRequestResultType(typeof(SampleMessage));

        Assert.Null(result);
    }

    [Fact]
    public void GetRequestResultType_RequestRegisteredViaAddActions_ReturnsResultType()
    {
        var sut = new ReflectionCache().AddActions(typeof(SampleRequest));

        var result = sut.GetRequestResultType(typeof(SampleRequest));

        Assert.Equal(typeof(string), result);
    }

    [Fact]
    public void GetHandlerExecutorType_ActionNeverRegistered_FallsBackToRuntimeResolution()
    {
        var sut = new ReflectionCache();

        var result = sut.GetHandlerExecutorType(typeof(SampleRequest));

        Assert.Equal(typeof(RequestHandlerExecutor<SampleRequest, string>), result);
    }

    [Fact]
    public void GetRequestResultType_ActionNeverRegistered_FallsBackToRuntimeResolution()
    {
        var sut = new ReflectionCache();

        var result = sut.GetRequestResultType(typeof(SampleRequest));

        Assert.Equal(typeof(string), result);
    }

    [Fact]
    public void GetHandlerExecutorType_UnregisteredActionResolvedTwice_ReturnsConsistentTypeFromRuntimeCache()
    {
        var sut = new ReflectionCache();

        var first = sut.GetHandlerExecutorType(typeof(SampleRequest));
        var second = sut.GetHandlerExecutorType(typeof(SampleRequest));

        Assert.Equal(first, second);
    }

    [Fact]
    public void GetActionTypes_AfterAddActions_ReturnsOnlyRegisteredTypes()
    {
        var sut = new ReflectionCache().AddActions(typeof(SampleMessage), typeof(SampleRequest));

        var result = sut.GetActionTypes();

        Assert.Equal(new[] { typeof(SampleMessage), typeof(SampleRequest) }, result, EqualityComparer<Type>.Default);
    }

    [Fact]
    public void GetMessageActionTypes_AfterAddActions_ReturnsOnlyActionsWithoutResultType()
    {
        var sut = new ReflectionCache().AddActions(typeof(SampleMessage), typeof(SampleRequest));

        var result = sut.GetMessageActionTypes();

        Assert.Equal([typeof(SampleMessage)], result);
    }

    [Fact]
    public void GetRequestActionTypes_AfterAddActions_ReturnsOnlyActionsWithResultType()
    {
        var sut = new ReflectionCache().AddActions(typeof(SampleMessage), typeof(SampleRequest));

        var result = sut.GetRequestActionTypes();

        Assert.Equal([typeof(SampleRequest)], result);
    }

    private class SampleMessage : IMediatorAction;

    private class SampleRequest : IMediatorAction<string>;
}
