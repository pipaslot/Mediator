using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Configuration;

/// <summary>
/// Verifies the HTTP GET allowlist members of <see cref="ServerMediatorOptions"/>: predicate-based registration via
/// <see cref="ServerMediatorOptions.AllowHttpGetWhen"/>/<see cref="ServerMediatorOptions.AllowHttpGetWhenAction{TAction}"/>,
/// and the computed <see cref="ServerMediatorOptions.RestrictHttpGetToAllowedActions"/> flag derived from whether any
/// filter was registered - there is no independent state to set it directly.
/// <para>
/// The previous type/assembly allowlist (AddAllowedHttpGetActionType/AssemblyOf/Assembly,
/// AllowedHttpGetActionTypes/Assemblies) no longer exists on <see cref="ServerMediatorOptions"/>; that is enforced by
/// this file compiling without them rather than by a runtime assertion (S8).
/// </para>
/// </summary>
public class ServerMediatorOptionsTests
{
    [Fact]
    public void RestrictHttpGetToAllowedActions_DefaultValue_IsFalse()
    {
        // With no filter ever registered, RestrictHttpGetToAllowedActions can never independently become true (it is
        // computed from the filter collection, not settable) - GET stays unrestricted rather than denying every
        // action, even though the flag conceptually reads as "on". (S5)
        var options = new ServerMediatorOptions();

        Assert.False(options.RestrictHttpGetToAllowedActions);
    }

    [Fact]
    public void AllowHttpGetWhen_RegistersFilterAndEnablesRestriction()
    {
        // (S6)
        var options = new ServerMediatorOptions();

        options.AllowHttpGetWhen(a => a is NopMessage);

        Assert.True(options.RestrictHttpGetToAllowedActions);
    }

    [Fact]
    public void AllowHttpGetWhenAction_AllowsMatchingTypeButNotOtherTypes()
    {
        var options = new ServerMediatorOptions();

        options.AllowHttpGetWhenAction<NopMessage>();

        Assert.True(options.IsAllowedOverHttpGet(new NopMessage()));
        Assert.False(options.IsAllowedOverHttpGet(new NopRequest()));
    }

    [Fact]
    public void IsAllowedOverHttpGet_WhenMultipleConditionsRegistered_AllowsActionIfAnyReturnsTrue()
    {
        // (S1/RE2 - OR-combination across conditions, not AND)
        var options = new ServerMediatorOptions();

        options.AllowHttpGetWhen(a => a is NopRequest);
        options.AllowHttpGetWhenAction<NopMessage>();

        Assert.True(options.IsAllowedOverHttpGet(new NopMessage()));
    }
}
