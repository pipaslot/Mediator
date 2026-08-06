using Pipaslot.Mediator.Http.Configuration;

namespace Pipaslot.Mediator.Http.Tests.Configuration;

/// <summary>
/// Verifies the members that are specific to <see cref="ServerMediatorOptions"/>. The shared
/// <see cref="BaseMediatorOptions{TBuilder}"/> surface (Endpoint normalization, credible-type registration) is
/// already covered through <see cref="ClientMediatorOptionsTests"/>, so it is not repeated here.
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
}
