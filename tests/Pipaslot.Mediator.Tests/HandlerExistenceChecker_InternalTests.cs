using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests;

public class HandlerExistenceChecker_InternalTests
{
    [Fact]
    public void FilterAssignableToRequest_RemoveMessage()
    {
        var expected = typeof(SingleHandler.Request);
        var types = new[] { typeof(SingleHandler.Message), expected, typeof(HandlerExistenceChecker_InternalTests) };
        var result = MediatorConfigurator.FilterAssignableToRequest(types);

        Assert.Contains(expected, result);
        Assert.Single(result);
    }


    [Fact]
    public void FilterAssignableToMessage_RemoveRequest()
    {
        var expected = typeof(SingleHandler.Message);
        var types = new[] { typeof(SingleHandler.Request), expected, typeof(HandlerExistenceChecker_InternalTests) };
        var result = MediatorConfigurator.FilterAssignableToMessage(types);

        Assert.Contains(expected, result);
        Assert.Single(result);
    }
}