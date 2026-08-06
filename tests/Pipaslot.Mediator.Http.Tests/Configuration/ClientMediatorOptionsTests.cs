using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Notifications;
using System.Reflection;

namespace Pipaslot.Mediator.Http.Tests.Configuration;

/// <summary>
/// Verifies option defaults and mutators inherited from <see cref="BaseMediatorOptions{TBuilder}"/>, exercised
/// through the concrete <see cref="ClientMediatorOptions"/> type since the base class is abstract, plus the
/// Client-specific <see cref="ClientMediatorOptions.AddContextAccessor"/> default override. Contrast with
/// <see cref="ServerMediatorOptionsTests"/>, which only covers members that differ on the server.
/// </summary>
public class ClientMediatorOptionsTests
{
    [Fact]
    public void Endpoint_DefaultValue_IsMediatorConstantsEndpoint()
    {
        var options = new ClientMediatorOptions();

        Assert.Equal(MediatorConstants.Endpoint, options.Endpoint);
    }

    [Fact]
    public void Endpoint_SetWithoutLeadingSlash_PrependsSlash()
    {
        var options = new ClientMediatorOptions();

        options.Endpoint = "api/_mediator/request";

        Assert.Equal("/api/_mediator/request", options.Endpoint);
    }

    [Fact]
    public void Endpoint_SetWithLeadingSlash_KeepsAsIs()
    {
        var options = new ClientMediatorOptions();

        options.Endpoint = "/api/_mediator/request";

        Assert.Equal("/api/_mediator/request", options.Endpoint);
    }

    [Fact]
    public void Endpoint_SetToNull_ResultsInSlashOnly()
    {
        var options = new ClientMediatorOptions();

        options.Endpoint = null!;

        Assert.Equal("/", options.Endpoint);
    }

    [Fact]
    public void AddContextAccessor_DefaultValue_IsFalse()
    {
        var options = new ClientMediatorOptions();

        Assert.False(options.AddContextAccessor);
    }

    [Fact]
    public void IgnoreReadOnlyProperties_DefaultValue_IsFalse()
    {
        var options = new ClientMediatorOptions();

        Assert.False(options.IgnoreReadOnlyProperties);
    }

    [Fact]
    public void DeserializeOnlyCredibleResultTypes_DefaultValue_IsFalse()
    {
        var options = new ClientMediatorOptions();

        Assert.False(options.DeserializeOnlyCredibleResultTypes);
    }

    [Fact]
    public void CredibleResultTypes_DefaultValue_ContainsOnlyNotification()
    {
        var options = new ClientMediatorOptions();

        Assert.Equal([typeof(Notification)], options.CredibleResultTypes);
    }

    [Fact]
    public void CredibleResultTypes_Set_ReplacesExistingValues()
    {
        var options = new ClientMediatorOptions();

        options.CredibleResultTypes = [typeof(string)];

        Assert.Equal([typeof(string)], options.CredibleResultTypes);
    }

    [Fact]
    public void CredibleResultAssemblies_DefaultValue_IsEmpty()
    {
        var options = new ClientMediatorOptions();

        Assert.Empty(options.CredibleResultAssemblies);
    }

    [Fact]
    public void CredibleResultAssemblies_Set_ReplacesExistingValues()
    {
        var options = new ClientMediatorOptions();
        var assembly = typeof(ClientMediatorOptions).Assembly;

        options.CredibleResultAssemblies = [assembly];

        Assert.Equal([assembly], options.CredibleResultAssemblies);
    }

    [Fact]
    public void AddCredibleResultType_Generic_AddsTypeToCredibleResultTypes()
    {
        var options = new ClientMediatorOptions();

        options.AddCredibleResultType<string>();

        Assert.Contains(typeof(string), options.CredibleResultTypes);
    }

    [Fact]
    public void AddCredibleResultType_Generic_EnablesDeserializeOnlyCredibleResultTypes()
    {
        var options = new ClientMediatorOptions();

        options.AddCredibleResultType<string>();

        Assert.True(options.DeserializeOnlyCredibleResultTypes);
    }

    [Fact]
    public void AddCredibleResultAssemblyOf_Generic_AddsAssemblyToCredibleResultAssemblies()
    {
        var options = new ClientMediatorOptions();

        options.AddCredibleResultAssemblyOf<ClientMediatorOptions>();

        Assert.Contains(typeof(ClientMediatorOptions).Assembly, options.CredibleResultAssemblies);
    }

    [Fact]
    public void AddCredibleResultAssemblyOf_Generic_EnablesDeserializeOnlyCredibleResultTypes()
    {
        var options = new ClientMediatorOptions();

        options.AddCredibleResultAssemblyOf<ClientMediatorOptions>();

        Assert.True(options.DeserializeOnlyCredibleResultTypes);
    }

    [Fact]
    public void AddCredibleResultAssembly_Params_AddsAssembliesToCredibleResultAssemblies()
    {
        var options = new ClientMediatorOptions();
        Assembly[] assemblies = [typeof(ClientMediatorOptions).Assembly, typeof(string).Assembly];

        options.AddCredibleResultAssembly(assemblies);

        Assert.Equal(assemblies, options.CredibleResultAssemblies);
    }

    [Fact]
    public void AddCredibleResultAssembly_Params_EnablesDeserializeOnlyCredibleResultTypes()
    {
        var options = new ClientMediatorOptions();

        options.AddCredibleResultAssembly(typeof(ClientMediatorOptions).Assembly);

        Assert.True(options.DeserializeOnlyCredibleResultTypes);
    }
}
