using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class RuleSetCollectionTests
    {
        [Theory]
        [InlineData(RuleOperator.And, true, true, true)]
        [InlineData(RuleOperator.And, false, true, false)]
        [InlineData(RuleOperator.And, true, false, false)]
        [InlineData(RuleOperator.And, false, false, false)]
        [InlineData(RuleOperator.Or, true, true, true)]
        [InlineData(RuleOperator.Or, false, true, true)]
        [InlineData(RuleOperator.Or, true, false, true)]
        [InlineData(RuleOperator.Or, false, false, false)]
        public void Granted(RuleOperator @operator, bool rule1, bool rule2, bool expected)
        {
            var sut = new RuleSetCollection(@operator,
                    new RuleSet(
                        new Rule("Role", "A1", rule1)
                    ),
                    new RuleSet(
                        new Rule("Role", "A1", rule2)
                    )
                );
            Assert.Equal(expected, sut.Granted);
        }

        [Theory]
        [InlineData(RuleOperator.And)]
        [InlineData(RuleOperator.Or)]
        public void StringifyNotGranted(RuleOperator @operator)
        {
            var hiddenSet = new RuleSet(
                RuleOperator.Or,
                new Rule("Role", "A1", true),
                new Rule("Claim", "A2")
                );
            var shownOrSet = new RuleSet(
                RuleOperator.Or,
                new Rule("Role", "A3"),
                new Rule("Claim", "A4")
                );
            var shownDuplicateOrSet = new RuleSet(
                RuleOperator.Or,
                new Rule("Role", "A5"),
                new Rule("Role", "A6")
                );
            var shownAndSet = new RuleSet(
                new Rule("Claim", "A7"),
                new Rule("Claim", "A8")
                );
            var collection = new RuleSetCollection(@operator, hiddenSet, shownOrSet, shownDuplicateOrSet, shownAndSet);
            var expected = "("
                + shownOrSet.StringifyNotGranted()
                + $" {@operator} "
                + shownDuplicateOrSet.StringifyNotGranted()
                + $" {@operator} "
                + shownAndSet.StringifyNotGranted()
                + ")";
            Assert.Equal(expected, collection.StringifyNotGranted());
        }
    }
}
