using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Tests.Authorization;

public class Rule_OperatorTests
{
    [Test]
    public async Task Add_TwoRules_AndRuleSet()
    {
        var combined = Rule.AllowOrDeny(true)
                       + Rule.AllowOrDeny(true);
        await AssertTwoRules(combined, Operator.Add);
    }

    [Test]
    public async Task And_TwoRules_AndRuleSet()
    {
        var combined = Rule.AllowOrDeny(true)
                       & Rule.AllowOrDeny(true);
        await AssertTwoRules(combined, Operator.And);
    }

    [Test]
    public async Task Or_TwoRules_OrRuleSet()
    {
        var combined = Rule.AllowOrDeny(true)
                       | Rule.AllowOrDeny(true);
        await AssertTwoRules(combined, Operator.Or);
    }

    private static async Task AssertTwoRules(RuleSet combined, Operator expectedOperator)
    {
        await Assert.That(combined.Operator).IsEqualTo(expectedOperator);
        await Assert.That(combined.Rules.Count).IsEqualTo(2);
    }
}