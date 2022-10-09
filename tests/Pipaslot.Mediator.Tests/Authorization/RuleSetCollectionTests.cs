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
            Assert.Equal(expected, sut.Granted);
        }

        [Theory]
        [InlineData(Operator.And)]
        [InlineData(Operator.Or)]
        public void StringifyNotGranted(Operator @operator)
        {
            var hiddenSet = RuleSet.Create(
                Operator.Or,
                new Rule("Role", "A1", true),
                new Rule("Claim", "A2")
                );
            var shownOrSet = RuleSet.Create(
                Operator.Or,
                new Rule("Role", "A3"),
                new Rule("Claim", "A4")
                );
            var shownDuplicateOrSet = RuleSet.Create(
                Operator.Or,
                new Rule("Role", "A5"),
                new Rule("Role", "A6")
                );
            var shownAndSet = new RuleSet(
                new Rule("Claim", "A7"),
                new Rule("Claim", "A8")
                );
            var collection = RuleSet.Create(@operator, hiddenSet, shownOrSet, shownDuplicateOrSet, shownAndSet);
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
