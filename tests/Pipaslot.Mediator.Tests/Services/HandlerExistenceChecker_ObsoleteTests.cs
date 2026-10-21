#pragma warning disable CS0618 // The test inspects the obsolete markers themselves.
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Services;
using System;
using System.Reflection;

namespace Pipaslot.Mediator.Tests.Services;

/// <summary>
/// The infrastructure types behind <see cref="IHandlerExistenceChecker"/> are marked obsolete as a warning so
/// consumers are pointed to the supported interface before the types become internal. Unlike
/// <see cref="HandlerExistenceCheckerTests"/>, which verifies what the checker reports, this class verifies the
/// obsolete markers only.
/// </summary>
public class HandlerExistenceChecker_ObsoleteTests
{
    public static TheoryData<Type> InfrastructureTypes => new()
    {
        typeof(ReflectionCache),
        typeof(IActionTypeProvider),
        typeof(HandlerExistenceChecker)
    };

    [Theory]
    [MemberData(nameof(InfrastructureTypes))]
    public void Type_IsMarkedObsoleteWithWarningOnly(Type type)
    {
        var attribute = type.GetCustomAttribute<ObsoleteAttribute>();

        Assert.NotNull(attribute);
        Assert.False(attribute.IsError);
    }

    [Theory]
    [MemberData(nameof(InfrastructureTypes))]
    public void Type_ObsoleteMessage_PointsToIHandlerExistenceChecker(Type type)
    {
        var attribute = type.GetCustomAttribute<ObsoleteAttribute>();

        Assert.NotNull(attribute);
        Assert.Contains(nameof(IHandlerExistenceChecker), attribute.Message);
    }
}
