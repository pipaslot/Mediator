using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Xml.Linq;
using Xunit;

namespace Pipaslot.Mediator.Tests.Authorization.Formatting
{
    public class DefaultRuleFormatterTests
    {
        private DefaultRuleFormatter Create()
        {
            return new DefaultRuleFormatter();
        }

        [Theory]
        [InlineData(Operator.And, "(Sentence1 OR Sentence2) AND Sentence3 AND (Sentence4 AND Sentence5)")]
        [InlineData(Operator.Or, "(Sentence1 OR Sentence2) OR Sentence3 OR (Sentence4 AND Sentence5)")]
        public void Format_OnlyDefaultRules(Operator @operator, string expected)
        {
            var set1 = RuleSet.Create(
                Operator.Or,
                Rule.Allow(false, "Sentence1"),
                Rule.Allow(false, "Sentence2")
                );
            var set2 = RuleSet.Create(
                Operator.And,
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
        [InlineData(Operator.And, "(Role 'A3' is required. OR Claim 'A4' is required.) AND (Role 'A5' is required. AND Role 'A6' is required.)")] // Deny result
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
                Operator.And,
                new Rule(ClaimTypes.Role, "A5"), // has to be converted to Role
                new Rule("Role", "A6")
                );
            var collection = RuleSet.Create(@operator, set1, set2, set3);
            AssertEqual(expected, collection);
        }

        [Theory]
        [InlineData(Operator.And, "(Role 'A3' is required. OR Claim 'A4' is required.) AND (Role 'A5' is required. OR Role 'A6' is required.) AND (Claim 'A7' is required. AND Claim 'A8' is required.)")] // Deny result
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
        [InlineData(RuleOutcome.Allow, "")]
        [InlineData(RuleOutcome.Deny, "User has to be authenticated.")]
        public void Format_Authorization(RuleOutcome outcome, string expected)
        {
            var rule = new Rule(IdentityPolicy.AuthenticationPolicyName, IdentityPolicy.AuthenticatedValue, outcome);
            AssertEqual(expected, Operator.And, rule);
        }

        [Theory]
        [InlineData(Operator.And, $"Role 'A1' is required. AND Claim 'A2' is required.")]
        [InlineData(Operator.Or, $"Role 'A1' is required. OR Claim 'A2' is required.")]
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
        [InlineData(Operator.And, $"Role 'A1' is required. AND Role 'A2' is required.")]
        [InlineData(Operator.Or, $"Role 'A1' is required. OR Role 'A2' is required.")]
        public void Format_TwoWithDuplicateName(Operator @operator, string expected)
        {
            AssertEqual(expected,
                @operator,
                new Rule("Role", "A1"),
                new Rule("Role", "A2"),
                new Rule("Ignored", "IgnoredValue", RuleOutcome.Ignored)
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

        [Fact]
        public void Format_NestedRuleWontBeWrappedBecauseItIsNotNecessary()
        {
            var nested = RuleSet.Create(Operator.And,
                Rule.Allow(false, "AAA"),
                Rule.Allow(false, "BBB")
                );
            var root = new RuleSet(nested);

            AssertEqual("AAA AND BBB", root);
        }

        [Fact]
        public void Format_NestedRuleAndIgnoredRuleWontBeWrappedBecauseItIsNotNecessary()
        {
            var nested = RuleSet.Create(Operator.And,
                Rule.Allow(false, "AAA"),
                Rule.Allow(false, "BBB")
                );
            var nested2 = RuleSet.Create(Operator.And,
                new Rule(RuleOutcome.Ignored,"")
                );
            var root = new RuleSet(nested, nested2);

            AssertEqual("AAA AND BBB", root);
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
