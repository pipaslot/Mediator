using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using System.Security.Claims;

namespace Pipaslot.Mediator.Tests.Authorization.Formatting;

public class DefaultNodeFormatterTests
{
    private DefaultNodeFormatter Create(bool clearNegativeMessages = true, MultipleRuleWrapType wrapType = MultipleRuleWrapType.Always)
    {
        var formatter = new DefaultNodeFormatter();
        if (clearNegativeMessages)
        {
            formatter.NegativeOutcomeMessagePrefix = string.Empty;
        }

        formatter.MultipleRuleWrapType = wrapType;
        return formatter;
    }

    [Test]
    [Arguments(Operator.Add, "(Sentence1 OR Sentence2) Sentence3 (Sentence4 AND Sentence5)")]
    [Arguments(Operator.And, "(Sentence1 OR Sentence2) AND Sentence3 AND (Sentence4 AND Sentence5)")]
    [Arguments(Operator.Or, "(Sentence1 OR Sentence2) OR Sentence3 OR (Sentence4 AND Sentence5)")]
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

    [Test]
    // Add
    [Arguments(MultipleRuleWrapType.Never, Operator.Add, Operator.Add, "Sentence1 Sentence2 Sentence3 Sentence4")]
    [Arguments(MultipleRuleWrapType.Never, Operator.Add, Operator.And, "Sentence1 AND Sentence2 Sentence3 AND Sentence4")]
    [Arguments(MultipleRuleWrapType.Never, Operator.Add, Operator.Or, "Sentence1 OR Sentence2 Sentence3 OR Sentence4")]
    // And
    [Arguments(MultipleRuleWrapType.Never, Operator.And, Operator.Add, "Sentence1 Sentence2 AND Sentence3 Sentence4")]
    [Arguments(MultipleRuleWrapType.Never, Operator.And, Operator.And, "Sentence1 AND Sentence2 AND Sentence3 AND Sentence4")]
    [Arguments(MultipleRuleWrapType.Never, Operator.And, Operator.Or, "Sentence1 OR Sentence2 AND Sentence3 OR Sentence4")]
    // Or
    [Arguments(MultipleRuleWrapType.Never, Operator.Or, Operator.Add, "Sentence1 Sentence2 OR Sentence3 Sentence4")]
    [Arguments(MultipleRuleWrapType.Never, Operator.Or, Operator.And, "Sentence1 AND Sentence2 OR Sentence3 AND Sentence4")]
    [Arguments(MultipleRuleWrapType.Never, Operator.Or, Operator.Or, "Sentence1 OR Sentence2 OR Sentence3 OR Sentence4")]
    // Add
    [Arguments(MultipleRuleWrapType.Always, Operator.Add, Operator.Add, "(Sentence1 Sentence2) (Sentence3 Sentence4)")]
    [Arguments(MultipleRuleWrapType.Always, Operator.Add, Operator.And, "(Sentence1 AND Sentence2) (Sentence3 AND Sentence4)")]
    [Arguments(MultipleRuleWrapType.Always, Operator.Add, Operator.Or, "(Sentence1 OR Sentence2) (Sentence3 OR Sentence4)")]
    // And
    [Arguments(MultipleRuleWrapType.Always, Operator.And, Operator.Add, "(Sentence1 Sentence2) AND (Sentence3 Sentence4)")]
    [Arguments(MultipleRuleWrapType.Always, Operator.And, Operator.And, "(Sentence1 AND Sentence2) AND (Sentence3 AND Sentence4)")]
    [Arguments(MultipleRuleWrapType.Always, Operator.And, Operator.Or, "(Sentence1 OR Sentence2) AND (Sentence3 OR Sentence4)")]
    // Or
    [Arguments(MultipleRuleWrapType.Always, Operator.Or, Operator.Add, "(Sentence1 Sentence2) OR (Sentence3 Sentence4)")]
    [Arguments(MultipleRuleWrapType.Always, Operator.Or, Operator.And, "(Sentence1 AND Sentence2) OR (Sentence3 AND Sentence4)")]
    [Arguments(MultipleRuleWrapType.Always, Operator.Or, Operator.Or, "(Sentence1 OR Sentence2) OR (Sentence3 OR Sentence4)")]
    // Add
    [Arguments(MultipleRuleWrapType.DifferentOperator, Operator.Add, Operator.Add, "Sentence1 Sentence2 Sentence3 Sentence4")]
    [Arguments(MultipleRuleWrapType.DifferentOperator, Operator.Add, Operator.And, "(Sentence1 AND Sentence2) (Sentence3 AND Sentence4)")]
    [Arguments(MultipleRuleWrapType.DifferentOperator, Operator.Add, Operator.Or, "(Sentence1 OR Sentence2) (Sentence3 OR Sentence4)")]
    // And
    [Arguments(MultipleRuleWrapType.DifferentOperator, Operator.And, Operator.Add, "(Sentence1 Sentence2) AND (Sentence3 Sentence4)")]
    [Arguments(MultipleRuleWrapType.DifferentOperator, Operator.And, Operator.And, "Sentence1 AND Sentence2 AND Sentence3 AND Sentence4")]
    [Arguments(MultipleRuleWrapType.DifferentOperator, Operator.And, Operator.Or, "(Sentence1 OR Sentence2) AND (Sentence3 OR Sentence4)")]
    // Or
    [Arguments(MultipleRuleWrapType.DifferentOperator, Operator.Or, Operator.Add, "(Sentence1 Sentence2) OR (Sentence3 Sentence4)")]
    [Arguments(MultipleRuleWrapType.DifferentOperator, Operator.Or, Operator.And, "(Sentence1 AND Sentence2) OR (Sentence3 AND Sentence4)")]
    [Arguments(MultipleRuleWrapType.DifferentOperator, Operator.Or, Operator.Or, "Sentence1 OR Sentence2 OR Sentence3 OR Sentence4")]
    public void Format_CombineRuleSetsWithMultipleRulesAndApplyWrapType(MultipleRuleWrapType wrapType, Operator parentOperator,
        Operator childOperator, string expected)
    {
        var set1 = RuleSet.Create(
            childOperator,
            Rule.Deny("Sentence1"),
            Rule.Deny("Sentence2")
        );
        var set2 = RuleSet.Create(
            childOperator,
            Rule.Deny("Sentence3"),
            Rule.Deny("Sentence4")
        );

        var collection = RuleSet.Create(parentOperator, set1, set2);
        AssertEqual(expected, collection, wrapType);
    }

    [Test]
    [Arguments(Operator.Add,
        "(Role 'A3' is required. OR Claim 'A4' is required.) (Role 'A5' is required. AND Role 'A6' is required.)")] // Deny result
    [Arguments(Operator.And,
        "(Role 'A3' is required. OR Claim 'A4' is required.) AND (Role 'A5' is required. AND Role 'A6' is required.)")] // Deny result
    [Arguments(Operator.Or, "")] // Allow result
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

    [Test]
    [Arguments(Operator.Add,
        "(Role 'A3' is required. OR Claim 'A4' is required.) (Role 'A5' is required. OR Role 'A6' is required.) (Claim 'A7' is required. AND Claim 'A8' is required.)")] // Deny result
    [Arguments(Operator.And,
        "(Role 'A3' is required. OR Claim 'A4' is required.) AND (Role 'A5' is required. OR Role 'A6' is required.) AND (Claim 'A7' is required. AND Claim 'A8' is required.)")] // Deny result
    [Arguments(Operator.Or, "You did it!")] // Allow result
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

    [Test]
    [Arguments(Operator.Add)]
    [Arguments(Operator.And)]
    public void Format_Single(Operator @operator)
    {
        var expected = "Role 'Admin' is required.";
        AssertEqual(expected, @operator
            , new Rule("Role", "Admin")
            , new Rule("Ignored", "IgnoredValue", true)
        );
    }

    [Test]
    [Arguments(Operator.And, RuleOutcome.Allow, "")]
    [Arguments(Operator.Add, RuleOutcome.Allow, "")]
    [Arguments(Operator.And, RuleOutcome.Deny, "User has to be authenticated.")]
    [Arguments(Operator.Add, RuleOutcome.Deny, "User has to be authenticated.")]
    public void Format_Authorization(Operator @operator, RuleOutcome outcome, string expected)
    {
        var rule = new Rule(IdentityPolicy.AuthenticationPolicyName, IdentityPolicy.AuthenticatedValue, outcome);
        AssertEqual(expected, @operator, rule);
    }

    [Test]
    [Arguments(Operator.Add, "Role 'A1' is required. Claim 'A2' is required.")]
    [Arguments(Operator.And, "Role 'A1' is required. AND Claim 'A2' is required. AND Ignored 'IgnoredValue' is required.")]
    [Arguments(Operator.Or, "Role 'A1' is required. OR Claim 'A2' is required.")]
    public void Format_TwoWithUniqueName(Operator @operator, string expected)
    {
        AssertEqual(expected,
            @operator,
            new Rule("Role", "A1"),
            new Rule("Claim", "A2"),
            new Rule("Ignored", "IgnoredValue", RuleOutcome.Ignored)
        );
    }

    [Test]
    [Arguments(Operator.Add, "Role 'A1' is required. Role 'A2' is required.")]
    [Arguments(Operator.And, "Role 'A1' is required. AND Role 'A2' is required. AND Ignored 'IgnoredValue' is required.")]
    [Arguments(Operator.Or, "Role 'A1' is required. OR Role 'A2' is required.")]
    public void Format_TwoWithDuplicateName(Operator @operator, string expected)
    {
        AssertEqual(expected,
            @operator,
            new Rule("Role", "A1"),
            new Rule("Role", "A2"),
            new Rule("Ignored", "IgnoredValue", RuleOutcome.Ignored)
        );
    }

    [Test]
    [Arguments(Operator.Add, "", "", "", "")]
    [Arguments(Operator.And, "", "", "", "")]
    [Arguments(Operator.Add, "", "AHA", "", "AHA")] // Format as single
    [Arguments(Operator.And, "", "AHA", "", "AHA")] // Format as single
    [Arguments(Operator.Add, "AHA", "", "BBB", "AHA BBB")] // Format as multiple
    [Arguments(Operator.And, "AHA", "", "BBB", "AHA AND BBB")] // Format as multiple
    public void Format_EmptyRuleIsIgnored(Operator @operator, string first, string second, string third, string expected)
    {
        AssertEqual(expected, @operator
            , Rule.Deny(first)
            , Rule.Deny(second)
            , Rule.Deny(third)
        );
    }

    [Test]
    [Arguments(Operator.Add, "AAA BBB")]
    [Arguments(Operator.And, "AAA AND BBB")]
    public void Format_NestedRuleWontBeWrappedBecauseItIsNotNecessary(Operator @operator, string expected)
    {
        var nested = RuleSet.Create(@operator,
            Rule.Deny("AAA"),
            Rule.Deny("BBB")
        );
        var root = new RuleSet(nested);

        AssertEqual(expected, root);
    }

    [Test]
    [Arguments(Operator.Add, "AAA BBB")]
    [Arguments(Operator.And, "AAA AND BBB")]
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

    [Test]
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

    private void AssertEqual(string expected, RuleSet ruleSet, MultipleRuleWrapType wrapType = MultipleRuleWrapType.Always)
    {
        var sut = Create(true, wrapType);
        var node = ruleSet.Reduce();
        var reason = sut.Format(node);
        Assert.Equal(expected, reason);
    }
}