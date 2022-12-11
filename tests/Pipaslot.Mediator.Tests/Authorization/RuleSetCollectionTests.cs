using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class RuleSetCollectionTests
    {
        [Theory]
        [InlineData(Operator.And, true, true, true)]
        [InlineData(Operator.And, false, true, false)]
        [InlineData(Operator.And, true, false, false)]
        [InlineData(Operator.And, false, false, false)]
        [InlineData(Operator.Or, true, true, true)]
        [InlineData(Operator.Or, false, true, true)]
        [InlineData(Operator.Or, true, false, true)]
        [InlineData(Operator.Or, false, false, false)]
        public void Granted(Operator @operator, bool rule1, bool rule2, bool expected)
        {
            var sut = RuleSet.Create(@operator,
                new RuleSet(
                    new Rule("Role", "A1", rule1)
                ),
                new RuleSet(
                    new Rule("Role", "A1", rule2)
                )
            );
            Assert.Equal(expected, sut.IsGranted());
        }
    }
}
