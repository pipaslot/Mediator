using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Xml.Linq;

namespace Pipaslot.Mediator.Tests.Authorization.Formatting
{
    public class DefaultRuleSetFormatterTests
    {
        private DefaultRuleSetFormatter Create()
        {
            return new DefaultRuleSetFormatter();
        }

        [Theory]
        [InlineData(Operator.And, "(Sentence1 OR Sentence2) AND Sentence3 AND (Sentence4 AND Sentence5)")] //Deny result
        [InlineData(Operator.Or, "")] // Allow result
        public void Format_OnlyDefaultRules(Operator @operator, string expected)
        {
            var set1 = RuleSet.Create(
                Operator.Or,
                Rule.Allow(false, "Sentence1"),
                Rule.Allow(false, "Sentence2")
                );
            var set2 = RuleSet.Create(
                Operator.Or,
                Rule.Allow(false, "Sentence3"),
                Rule.Allow(true, "Ignored sentence because it is granted")
                );
            var set3 = RuleSet.Create(
                Operator.And,
                Rule.Allow(false, "Sentence4"),
                Rule.Allow(false, "Sentence5")
                );

            var collection = RuleSet.Create(@operator, set1, set2, set3);
            AssertEqual(expected, collection);
        }

        [Theory]
        [InlineData(Operator.And, "({'Role': 'A3'} OR {'Claim': 'A4'}) AND {'Role': ['A5' OR 'A6']} AND {'Claim': ['A7' AND 'A8']}")] // Deny result
        [InlineData(Operator.Or, "")] // Allow result
        public void Format_withoutDefaultRule(Operator @operator, string expected)
        {
            var set1 = RuleSet.Create(
                Operator.Or,
                new Rule("Role", "A1", true),
                new Rule("Claim", "A2")
                );
            var set2 = RuleSet.Create(
                Operator.Or,
                new Rule("Role", "A3"),
                new Rule("Claim", "A4")
                );
            var set3 = RuleSet.Create(
                Operator.Or,
                new Rule("Role", "A5"),
                new Rule("Role", "A6")
                );
            var set4 = new RuleSet(
                new Rule("Claim", "A7"),
                new Rule("Claim", "A8")
                );
            var collection = RuleSet.Create(@operator, set1, set2, set3, set4);
            AssertEqual(expected, collection);
        }

        [Theory]
        [InlineData(Operator.And, "({'Role': 'A3'} OR {'Claim': 'A4'}) AND {'Role': ['A5' OR 'A6']} AND {'Claim': ['A7' AND 'A8']}")] // Deny result
        [InlineData(Operator.Or, "You did it!")] // Allow result
        public void Format_WithDefaultRule(Operator @operator, string expected)
        {
            var hiddenSet = RuleSet.Create(
                Operator.Or,
                Rule.Allow(true, "", "You did it!"),
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
            AssertEqual(expected, collection);
        }

        [Fact]
        public void Format_Single()
        {
            var expected = $"Role 'Admin' is required.";
            AssertEqual(expected, Operator.And
                , new Rule("Role", "Admin")
                , new Rule("Ignored", "IgnoredValue", true)
                );
        }

        [Theory]
        [InlineData(Operator.And, $"{{'Role': 'A1'}} AND {{'Claim': 'A2'}}")]
        [InlineData(Operator.Or, $"{{'Role': 'A1'}} OR {{'Claim': 'A2'}}")]
        public void Format_TwoWithUniqueName(Operator @operator, string expected)
        {
            AssertEqual(expected,
                @operator,
                new Rule("Role", "A1"),
                new Rule("Claim", "A2"),
                new Rule("Ignored", "IgnoredValue", RuleOutcome.Ignored)
                );
        }

        [Theory]
        [InlineData(Operator.And, true, $"{{'Role': ['A1' AND 'A2']}}")]
        [InlineData(Operator.And, false, $"{{'Role': ['A1' AND 'A2']}}")]
        [InlineData(Operator.Or, true, $"{{'Role': ['A1' OR 'A2']}}")]
        [InlineData(Operator.Or, false, $"{{'Role': ['A1' OR 'A2']}}")]
        public void Format_TwoWithDuplicateName(Operator @operator, bool theSameNameCase, string expected)
        {
            var name = "Role";
            AssertEqual(expected,
                @operator,
                new Rule(name, "A1"),
                new Rule(theSameNameCase ? name : name.ToUpper(), "A2"),
                new Rule("Ignored", "IgnoredValue", RuleOutcome.Ignored)
                );
        }

        [Fact]
        public void Format_CollectionWthSingleRuleSet_ReturnOnlySet()
        {
            var expected = "{'Role': 'A1'} AND {'Claim': 'A2'}";
            AssertEqual(expected, Operator.And
                , new Rule("Role", "A1")
                , new Rule("Claim", "A2")
                );
        }

        [Theory]
        [InlineData("", "", "", "")]
        [InlineData("", "AHA", "", "AHA")] // Format as single
        [InlineData("AHA", "", "BBB", "AHA AND BBB")] // Format as multiple
        public void Format_EmptyRuleIsIgnored(string first, string second, string third, string expected)
        {
            AssertEqual(expected, Operator.And
                , Rule.Allow(false, first)
                , Rule.Allow(false, second)
                , Rule.Allow(false, third)
                );
        }

        private void AssertEqual(string expected, Operator @operator, params Rule[] rules)
        {
            var set = RuleSet.Create(@operator, rules);
            AssertEqual(expected, set);
        }

        private void AssertEqual(string expected, RuleSet ruleSet)
        {
            var sut = Create();
            var eval = ruleSet.Evaluate(sut);
            Assert.Equal(expected, eval.Value);
        }
    }
}
