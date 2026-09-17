using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Configuration;

/// <summary>
/// Verifies the HTTP GET allowlist members of <see cref="ServerMediatorOptions"/>: the default is permissive
/// (unchanged pre-existing behavior), and each Add* registration method both records the entry and flips
/// <see cref="ServerMediatorOptions.RestrictHttpGetToAllowedActionTypes"/> on as a side effect.
/// </summary>
public class ServerMediatorOptionsTests
{
    [Fact]
    public void RestrictHttpGetToAllowedActionTypes_DefaultValue_IsFalse()
    {
        var options = new ServerMediatorOptions();

        Assert.False(options.RestrictHttpGetToAllowedActionTypes);
    }

    [Fact]
    public void AddAllowedHttpGetActionType_RegistersTypeAndEnablesRestriction()
    {
        var options = new ServerMediatorOptions();

        options.AddAllowedHttpGetActionType<NopMessage>();

        Assert.True(options.RestrictHttpGetToAllowedActionTypes);
        Assert.Contains(typeof(NopMessage), options.AllowedHttpGetActionTypes);
    }

    [Fact]
    public void AddAllowedHttpGetActionAssemblyOf_RegistersAssemblyAndEnablesRestriction()
    {
        var options = new ServerMediatorOptions();

        options.AddAllowedHttpGetActionAssemblyOf<NopMessage>();

        Assert.True(options.RestrictHttpGetToAllowedActionTypes);
        Assert.Contains(typeof(NopMessage).Assembly, options.AllowedHttpGetActionAssemblies);
    }

    [Fact]
    public void AddAllowedHttpGetActionAssembly_RegistersAssemblyAndEnablesRestriction()
    {
        var options = new ServerMediatorOptions();

        options.AddAllowedHttpGetActionAssembly(typeof(NopMessage).Assembly);

        Assert.True(options.RestrictHttpGetToAllowedActionTypes);
        Assert.Contains(typeof(NopMessage).Assembly, options.AllowedHttpGetActionAssemblies);
    }
}
