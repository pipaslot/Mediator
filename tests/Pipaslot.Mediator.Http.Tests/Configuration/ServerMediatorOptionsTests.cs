using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using System;

namespace Pipaslot.Mediator.Http.Tests.Configuration;

/// <summary>
/// Verifies the members that are specific to <see cref="ServerMediatorOptions"/>. The shared
/// <see cref="BaseMediatorOptions{TBuilder}"/> surface (Endpoint normalization, credible-type registration) is
/// already covered through <see cref="ClientMediatorOptionsTests"/>, so it is not repeated here.
/// It also covers the HTTP GET allowlist exposed through <see cref="ServerMediatorOptions.HttpGetConditions"/>:
/// predicate-based registration via <see cref="HttpGetActionAllowlist.Allow"/>, clearing via
/// <see cref="HttpGetActionAllowlist.Clear"/>, and the resulting allow/deny decision from
/// <see cref="HttpGetActionAllowlist.IsAllowed"/>. Whether GET is restricted at all follows purely from whether any
/// condition was ever registered; there is no separate settable flag for it.
/// </summary>
public class ServerMediatorOptionsTests
{
    [Fact]
    public void DeserializeOnlyCredibleActionTypes_DefaultValue_IsTrue()
    {
        var options = new ServerMediatorOptions();

        Assert.True(options.DeserializeOnlyCredibleActionTypes);
    }

    [Fact]
    public void ErrorHttpStatusCode_DefaultValue_IsMediatorConstantsErrorHttpStatusCode()
    {
        var options = new ServerMediatorOptions();

        Assert.Equal(MediatorConstants.ErrorHttpStatusCode, options.ErrorHttpStatusCode);
    }

    [Fact]
    public void AddContextAccessor_DefaultValue_IsTrue()
    {
        var options = new ServerMediatorOptions();

        Assert.True(options.AddContextAccessor);
    }

    [Fact]
    public void IsAllowed_WithNoConditionRegistered_AllowsAnyAction()
    {
        // With no condition ever registered, GET stays unrestricted for every action - unchanged from previous
        // versions. (S5)
        var options = new ServerMediatorOptions();

        Assert.True(options.HttpGetConditions.IsAllowed(new NopMessage()));
        Assert.True(options.HttpGetConditions.IsAllowed(new NopRequest()));
    }

    [Fact]
    public void IsAllowed_WhenConditionRegistered_AllowsOnlyMatchingAction()
    {
        var options = new ServerMediatorOptions();

        options.HttpGetConditions.Allow(a => a is NopMessage);

        Assert.True(options.HttpGetConditions.IsAllowed(new NopMessage()));
        Assert.False(options.HttpGetConditions.IsAllowed(new NopRequest()));
    }

    [Fact]
    public void IsAllowed_WhenMultipleConditionsRegistered_AllowsActionIfAnyReturnsTrue()
    {
        var options = new ServerMediatorOptions();

        options.HttpGetConditions.Allow(a => a is NopRequest);
        options.HttpGetConditions.Allow(a => a is NopMessage);

        Assert.True(options.HttpGetConditions.IsAllowed(new NopMessage()));
    }

    [Fact]
    public void Clear_AfterConditionRegistered_RestoresUnrestrictedDefault()
    {
        var options = new ServerMediatorOptions();
        options.HttpGetConditions.Allow(a => a is NopMessage);

        options.HttpGetConditions.Clear();

        Assert.True(options.HttpGetConditions.IsAllowed(new NopMessage()));
        Assert.True(options.HttpGetConditions.IsAllowed(new NopRequest()));
    }

    [Fact]
    public void Clear_ThenAllow_LetsALaterConfigurationStepReplaceAnEarlierOnesConditions()
    {
        var options = new ServerMediatorOptions();
        options.HttpGetConditions.Allow(a => a is NopMessage); // registered by an earlier configuration step

        options.HttpGetConditions.Clear(); // a later step overrides it entirely
        options.HttpGetConditions.Allow(a => a is NopRequest);

        Assert.False(options.HttpGetConditions.IsAllowed(new NopMessage()));
        Assert.True(options.HttpGetConditions.IsAllowed(new NopRequest()));
    }

    [Fact]
    public void Conditions_AfterAllow_RoundTripsRegisteredCondition()
    {
        var options = new ServerMediatorOptions();
        Func<IMediatorAction, bool> condition = a => a is NopMessage;

        options.HttpGetConditions.Allow(condition);

        Assert.Same(condition, Assert.Single(options.HttpGetConditions));
    }

    public class NopMessage : IMessage;

    public class NopRequest : IRequest<string>;
}
