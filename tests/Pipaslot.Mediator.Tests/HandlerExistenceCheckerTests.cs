using Pipaslot.Mediator.Services;
using Pipaslot.Mediator.Tests.ValidActions;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class HandlerExistenceCheckerTests
    {
        [Fact]
        public void FilterAssignableToRequest_RemoveMessage()
        {
            var expected = typeof(SingleHandler.Request);
            var types = new[]
            {
                typeof(SingleHandler.Message),
                expected,
                typeof(HandlerExistenceCheckerTests)
            };
            var result = HandlerExistenceChecker.FilterAssignableToRequest(types);

            Assert.Contains(expected, result);
            Assert.Single(result);
        }


        [Fact]
        public void FilterAssignableToMessage_RemoveRequest()
        {
            var expected = typeof(SingleHandler.Message);
            var types = new[]
            {
                typeof(SingleHandler.Request),
                expected,
                typeof(HandlerExistenceCheckerTests)
            };
            var result = HandlerExistenceChecker.FilterAssignableToMessage(types);

            Assert.Contains(expected, result);
            Assert.Single(result);
        }
    }
}
