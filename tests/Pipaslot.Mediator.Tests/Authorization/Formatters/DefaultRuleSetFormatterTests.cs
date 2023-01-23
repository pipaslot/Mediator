using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatters;

namespace Pipaslot.Mediator.Tests.Authorization.Formatters
{
    public class DefaultRuleSetFormatterTests
    {
        private DefaultRuleSetFormatter Create()
        {
            return new DefaultRuleSetFormatter();
        }

        [Theory]
        [InlineData(Operator.And, "(({'Role': 'A3'} OR {'Claim': 'A4'}) AND {'Role': ['A5' OR 'A6']} AND {'Claim': ['A7' AND 'A8']})", RuleOutcome.Deny)]
        [InlineData(Operator.Or, "", RuleOutcome.Allow)]
        public void Format(Operator @operator, string expected, RuleOutcome outcome)
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
            var actualOutcome = AssertEqual(expected, collection);
            Assert.Equal(outcome, actualOutcome);
        }

        [Fact]
        public void Format_Single()
        {
            var set = new RuleSet(
                new Rule("Role", "Admin"),
                new Rule("Ignored", "IgnoredValue", true)
                );
            var expected = $"Role 'Admin' is required.";
            AssertEqual(expected, set);
        }

        [Theory]
        [InlineData(Operator.And, $"({{'Role': 'A1'}} AND {{'Claim': 'A2'}})")]
        [InlineData(Operator.Or, $"({{'Role': 'A1'}} OR {{'Claim': 'A2'}})")]
        public void Format_TwoWithUniqueName(Operator @operator, string expected)
        {
            var set = RuleSet.Create(
                @operator,
                new Rule("Role", "A1"),
                new Rule("Claim", "A2"),
                new Rule("Ignored", "IgnoredValue", RuleOutcome.Ignored)
                );
            AssertEqual(expected, set);
        }

        [Theory]
        [InlineData(Operator.And, true, $"{{'Role': ['A1' AND 'A2']}}")]
        [InlineData(Operator.And, false, $"{{'Role': ['A1' AND 'A2']}}")]
        [InlineData(Operator.Or, true, $"{{'Role': ['A1' OR 'A2']}}")]
        [InlineData(Operator.Or, false, $"{{'Role': ['A1' OR 'A2']}}")]
        public void Format_TwoWithDuplicateName(Operator @operator, bool theSameNameCase, string expected)
        {
            var name = "Role";
            var set = RuleSet.Create(
                @operator,
                new Rule(name, "A1"),
                new Rule(theSameNameCase ? name : name.ToUpper(), "A2"),
                new Rule("Ignored", "IgnoredValue", RuleOutcome.Ignored)
                );
            AssertEqual(expected, set);
        }

        [Fact]
        public void Format_CollectionWithSingleRuleSet_ReturnOnlySet()
        {
            var set = new RuleSet(
                new Rule("Role", "A1"),
                new Rule("Claim", "A2")
                );
            var expected = "({'Role': 'A1'} AND {'Claim': 'A2'})";
            AssertEqual(expected, set);
        }

        private RuleOutcome AssertEqual(string expected, RuleSet ruleSet)
        {
            var sut = Create();
            var eval = ruleSet.Evaluate(sut);
            Assert.Equal(expected, eval.Value);
            return eval.Outcome;
        }
    }
}
