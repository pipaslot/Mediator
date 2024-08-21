using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using System.Security.Claims;

namespace Pipaslot.Mediator.Tests.Authorization.Formatting
{
    public class DefaultNodeFormatterTests
    {
        private DefaultNodeFormatter Create(bool clearNegativeMessages = true)
        {
            var formatter = new DefaultNodeFormatter();
            if (clearNegativeMessages)
            {
                formatter.NegativeOutcomeMessagePrefix = string.Empty;
            }
            return formatter;
        }

        [Theory]
        [InlineData(Operator.Add, "(Sentence1 OR Sentence2) Sentence3 (Sentence4 AND Sentence5)")]
        [InlineData(Operator.And, "(Sentence1 OR Sentence2) AND Sentence3 AND (Sentence4 AND Sentence5)")]
        [InlineData(Operator.Or, "(Sentence1 OR Sentence2) OR Sentence3 OR (Sentence4 AND Sentence5)")]
        public void Format_OnlyDefaultRules(Operator @operator, string expected)
        {
            var set1 = RuleSet.Create(
                Operator.Or,
                Rule.Deny("Sentence1"),
                Rule.Deny("Sentence2")
                );
            var set2 = RuleSet.Create(
                Operator.Add,
                Rule.Deny("Sentence3"),
                Rule.AllowOrDeny(true, "", "Ignored sentence because it is granted")
                );
            var set3 = RuleSet.Create(
                Operator.And,
                Rule.Deny("Sentence4"),
                Rule.Deny("Sentence5")
                );

            var collection = RuleSet.Create(@operator, set1, set2, set3);
            AssertEqual(expected, collection);
        }

        [Theory]
        [InlineData(Operator.Add, "(Role 'A3' is required. OR Claim 'A4' is required.) (Role 'A5' is required. AND Role 'A6' is required.)")] // Deny result
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
        [InlineData(Operator.Add, "(Role 'A3' is required. OR Claim 'A4' is required.) (Role 'A5' is required. OR Role 'A6' is required.) (Claim 'A7' is required. AND Claim 'A8' is required.)")] // Deny result
        [InlineData(Operator.And, "(Role 'A3' is required. OR Claim 'A4' is required.) AND (Role 'A5' is required. OR Role 'A6' is required.) AND (Claim 'A7' is required. AND Claim 'A8' is required.)")] // Deny result
        [InlineData(Operator.Or, "You did it!")] // Allow result
        public void Format_WithDefaultRule(Operator @operator, string expected)
        {
            var hiddenSet = RuleSet.Create(
                Operator.Or,
                Rule.AllowOrDeny(true, "You did it!"),
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
            var shownAndSet = RuleSet.Create(
                Operator.And,
                new Rule("Claim", "A7"),
                new Rule("Claim", "A8")
                );
            var collection = RuleSet.Create(@operator, hiddenSet, shownOrSet, shownDuplicateOrSet, shownAndSet);
            AssertEqual(expected, collection);
        }

        [Theory]
        [InlineData(Operator.Add)]
        [InlineData(Operator.And)]
        public void Format_Single(Operator @operator)
        {
            var expected = $"Role 'Admin' is required.";
            AssertEqual(expected, @operator
                , new Rule("Role", "Admin")
                , new Rule("Ignored", "IgnoredValue", true)
                );
        }

        [Theory]
        [InlineData(Operator.And,RuleOutcome.Allow, "")]
        [InlineData(Operator.Add,RuleOutcome.Allow, "")]
        [InlineData(Operator.And,RuleOutcome.Deny, "User has to be authenticated.")]
        [InlineData(Operator.Add,RuleOutcome.Deny, "User has to be authenticated.")]
        public void Format_Authorization(Operator @operator, RuleOutcome outcome, string expected)
        {
            var rule = new Rule(IdentityPolicy.AuthenticationPolicyName, IdentityPolicy.AuthenticatedValue, outcome);
            AssertEqual(expected, @operator, rule);
        }

        [Theory]
        [InlineData(Operator.Add, $"Role 'A1' is required. Claim 'A2' is required.")]
        [InlineData(Operator.And, $"Role 'A1' is required. AND Claim 'A2' is required. AND Ignored 'IgnoredValue' is required.")]
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
        [InlineData(Operator.Add, $"Role 'A1' is required. Role 'A2' is required.")]
        [InlineData(Operator.And, $"Role 'A1' is required. AND Role 'A2' is required. AND Ignored 'IgnoredValue' is required.")]
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
        [InlineData(Operator.Add,"", "", "", "")]
        [InlineData(Operator.And,"", "", "", "")]
        [InlineData(Operator.Add,"", "AHA", "", "AHA")] // Format as single
        [InlineData(Operator.And,"", "AHA", "", "AHA")] // Format as single
        [InlineData(Operator.Add,"AHA", "", "BBB", "AHA BBB")] // Format as multiple
        [InlineData(Operator.And,"AHA", "", "BBB", "AHA AND BBB")] // Format as multiple
        public void Format_EmptyRuleIsIgnored(Operator @operator, string first, string second, string third, string expected)
        {
            AssertEqual(expected, @operator
                , Rule.Deny(first)
                , Rule.Deny(second)
                , Rule.Deny(third)
                );
        }

        [Theory]
        [InlineData(Operator.Add, "AAA BBB")]
        [InlineData(Operator.And, "AAA AND BBB")]
        public void Format_NestedRuleWontBeWrappedBecauseItIsNotNecessary(Operator @operator, string expected)
        {
            var nested = RuleSet.Create(@operator,
                Rule.Deny( "AAA"),
                Rule.Deny("BBB")
                );
            var root = new RuleSet(nested);

            AssertEqual(expected, root);
        }

        [Theory]
        [InlineData(Operator.Add, "AAA BBB")]
        [InlineData(Operator.And, "AAA AND BBB")]
        public void Format_NestedRuleAndIgnoredRuleWontBeWrappedBecauseItIsNotNecessary(Operator @operator, string expected)
        {
            var nested = RuleSet.Create(@operator,
                Rule.Deny("AAA"),
                Rule.Deny("BBB")
                );
            var nested2 = RuleSet.Create(@operator,
                new Rule(RuleOutcome.Ignored, "")
                );
            var root = new RuleSet(nested, nested2);

            AssertEqual(expected, root);
        }
        
        [Fact]
        public void Format_AllowWithReason_ReasonIsPropagated()
        {
            var reason = "Because system allows it";
            var root = RuleSet.Create(Operator.And,
                Rule.Allow(reason)
            );
            AssertEqual(reason, root);
        }

        private void AssertEqual(string expected, Operator @operator, params Rule[] rules)
        {
            var set = RuleSet.Create(@operator, rules);
            AssertEqual(expected, set);
        }

        private void AssertEqual(string expected, RuleSet ruleSet)
        {
            var sut = Create();
            var node = ruleSet.Reduce();
            var reason = sut.Format(node);
            Assert.Equal(expected, reason);
        }
    }
}
