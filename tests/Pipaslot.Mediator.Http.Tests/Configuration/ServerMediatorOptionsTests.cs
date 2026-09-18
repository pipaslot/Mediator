using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Configuration;

/// <summary>
/// Verifies the HTTP GET allowlist behavior of <see cref="ServerMediatorOptions"/>: predicate-based registration via
/// <see cref="ServerMediatorOptions.AllowHttpGetWhen"/> and <see cref="ServerMediatorOptions.HttpGetConditions"/>,
/// and the resulting (internal) allow/deny decision exposed through
/// <see cref="ServerMediatorOptions.IsAllowedOverHttpGet"/>. Whether GET is restricted at all follows purely from
/// whether any condition was ever registered; there is no separate settable flag for it.
/// <para>
/// The previous type/assembly allowlist (AddAllowedHttpGetActionType/AssemblyOf/Assembly,
/// AllowedHttpGetActionTypes/Assemblies) no longer exists on <see cref="ServerMediatorOptions"/>; that is enforced by
/// this file compiling without them rather than by a runtime assertion (S8).
/// </para>
/// </summary>
public class ServerMediatorOptionsTests
{
    [Fact]
    public void IsAllowedOverHttpGet_WithNoConditionRegistered_AllowsAnyAction()
    {
        // With no condition ever registered, GET stays unrestricted for every action - unchanged from previous
        // versions. (S5)
        var options = new ServerMediatorOptions();

        Assert.True(options.IsAllowedOverHttpGet(new NopMessage()));
        Assert.True(options.IsAllowedOverHttpGet(new NopRequest()));
    }

    [Fact]
    public void IsAllowedOverHttpGet_WhenConditionRegistered_AllowsOnlyMatchingAction()
    {
        var options = new ServerMediatorOptions();

        options.AllowHttpGetWhen(a => a is NopMessage);

        Assert.True(options.IsAllowedOverHttpGet(new NopMessage()));
        Assert.False(options.IsAllowedOverHttpGet(new NopRequest()));
    }

    [Fact]
    public void IsAllowedOverHttpGet_WhenMultipleConditionsRegistered_AllowsActionIfAnyReturnsTrue()
    {
        var options = new ServerMediatorOptions();

        options.AllowHttpGetWhen(a => a is NopRequest);
        options.AllowHttpGetWhen(a => a is NopMessage);

        Assert.True(options.IsAllowedOverHttpGet(new NopMessage()));
    }

    [Fact]
    public void HttpGetConditions_SetAfterAllowHttpGetWhen_ReplacesEarlierConditionsWholesale()
    {
        var options = new ServerMediatorOptions();
        options.AllowHttpGetWhen(a => a is NopMessage);

        options.HttpGetConditions = [a => a is NopRequest];

        Assert.False(options.IsAllowedOverHttpGet(new NopMessage()));
        Assert.True(options.IsAllowedOverHttpGet(new NopRequest()));
    }

    [Fact]
    public void HttpGetConditions_SetToEmpty_RestoresUnrestrictedDefault()
    {
        var options = new ServerMediatorOptions();
        options.AllowHttpGetWhen(a => a is NopMessage);

        options.HttpGetConditions = [];

        Assert.True(options.IsAllowedOverHttpGet(new NopMessage()));
        Assert.True(options.IsAllowedOverHttpGet(new NopRequest()));
    }

    [Fact]
    public void HttpGetConditions_AfterAllowHttpGetWhen_RoundTripsRegisteredCondition()
    {
        var options = new ServerMediatorOptions();
        Func<IMediatorAction, bool> condition = a => a is NopMessage;

        options.AllowHttpGetWhen(condition);

        Assert.Same(condition, Assert.Single(options.HttpGetConditions));
    }
}
