using System.Text.Json;
using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Tests.Authorization;

public class RuleSetSerializationTests
{
    [Fact]
    public void RuleSet_RoundtripSerialization_RestoresEntireState()
    {
        // Arrange: create a complex structure with nested RuleSets and various Rules
        var root = new RuleSet(Operator.Or, [
            // First child: Add with two different rules
            new RuleSet(Operator.Add, [
                new Rule(name: "Role", value: "Admin", outcome: RuleOutcome.Allow, scope: RuleScope.Identity),
                new Rule(name: "Claim", value: "CanEdit", outcome: RuleOutcome.Deny, scope: RuleScope.State)
            ]),

            // Second child: And with nested Or set inside
            new RuleSet(Operator.And, [
                new RuleSet(Operator.Or, [
                    new Rule(name: "Feature", value: "ExperimentalX", outcome: RuleOutcome.Unavailable, scope: RuleScope.State),
                    new Rule(name: "Toggle", value: "Beta", outcome: RuleOutcome.Ignored, scope: RuleScope.Identity)
                ])
            ])
        ]);

        // Act: serialize and then deserialize using a converter for Rule
        var json = JsonSerializer.Serialize(root);
        var clone = JsonSerializer.Deserialize<RuleSet>(json)!;

        // Assert: entire state is restored (operators, rules, and all rule properties)
        AssertRuleSetEqual(root, clone);
    }

    private static void AssertRuleSetEqual(RuleSet expected, RuleSet actual)
    {
        Assert.Equal(expected.Operator, actual.Operator);

        // Compare rules in order
        Assert.Equal(expected.Rules.Count, actual.Rules.Count);
        for (var i = 0; i < expected.Rules.Count; i++)
        {
            var er = expected.Rules[i];
            var ar = actual.Rules[i];
            Assert.Equal(er.Name, ar.Name);
            Assert.Equal(er.Value, ar.Value);
            Assert.Equal(er.Scope, ar.Scope);
            Assert.Equal(er.Outcome, ar.Outcome);
        }

        // Compare child rule sets recursively in order
        Assert.Equal(expected.RuleSets.Count, actual.RuleSets.Count);
        for (var i = 0; i < expected.RuleSets.Count; i++)
        {
            AssertRuleSetEqual(expected.RuleSets[i], actual.RuleSets[i]);
        }
    }
}