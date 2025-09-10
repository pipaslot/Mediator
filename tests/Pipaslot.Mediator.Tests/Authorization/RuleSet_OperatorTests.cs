using Pipaslot.Mediator.Authorization;
using System.Linq;

namespace Pipaslot.Mediator.Tests.Authorization;

public class RuleSet_OperatorTests
{
    [Test]
    public async Task ADD_ThreeTimeTheSame()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1 + r2 + r3;
        await AssertRuleSet(combined, Operator.Add, 1, r3);
        var subSet = combined.RuleSets.First();
        await AssertRuleSet(subSet, Operator.Add, 0, r1, r2);
    }

    [Test]
    public async Task AND_ThreeTimeTheSame()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1 & r2 & r3;
        await AssertRuleSet(combined, Operator.And, 1, r3);
        var subSet = combined.RuleSets.First();
        await AssertRuleSet(subSet, Operator.And, 0, r1, r2);
    }

    [Test]
    public async Task OR_ThreeTimeTheSame()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1 | r2 | r3;
        await AssertRuleSet(combined, Operator.Or, 1, r3);
        var subSet = combined.RuleSets.First();
        await AssertRuleSet(subSet, Operator.Or, 0, r1, r2);
    }

    [Test]
    public async Task ORADDWithBrackets_RuleSetConbinedWitRuleByOrAndWithInverted_AppendTheRule()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = (r1
                        | r2)
                       + r3;
        await AssertRuleSet(combined, Operator.Add, 1, r3);
        var subSet = combined.RuleSets.First();
        await AssertRuleSet(subSet, Operator.Or, 0, r1, r2);
    }


    [Test]
    public async Task ORANDWithBrackets_RuleSetConbinedWitRuleByOrAndWithInverted_AppendTheRule()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = (r1
                        | r2)
                       & r3;
        await AssertRuleSet(combined, Operator.And, 1, r3);
        var subSet = combined.RuleSets.First();
        await AssertRuleSet(subSet, Operator.Or, 0, r1, r2);
    }

    [Test]
    public async Task ORADD_RuleSetConbinedWitRuleByOrAndWithInverted_AppendTheRule()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1
                       | (r2
                          + r3);
        await AssertRuleSet(combined, Operator.Or, 1, r1);
        var subSet = combined.RuleSets.First();
        await AssertRuleSet(subSet, Operator.Add, 0, r2, r3);
    }

    [Test]
    public async Task ORAND_RuleSetConbinedWitRuleByOrAndWithInverted_AppendTheRule()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1
                       | (r2
                          & r3);
        await AssertRuleSet(combined, Operator.Or, 1, r1);
        var subSet = combined.RuleSets.First();
        await AssertRuleSet(subSet, Operator.And, 0, r2, r3);
    }

    [Test]
    public async Task CastCombinedRulesToPolicy()
    {
        IPolicy combined = Rule.AllowOrDeny(true)
                           | ((Rule.AllowOrDeny(true)
                               + Rule.AllowOrDeny(true))
                              & Rule.AllowOrDeny(true))
                           | Rule.AllowOrDeny(true);

        await Assert.That(combined).IsNotNull();
    }

    [Test]
    public async Task CastCombinedRuleSetsToPolicy()
    {
        var one = Rule.AllowOrDeny(true)
                  | Rule.AllowOrDeny(true);
        var two = Rule.AllowOrDeny(true)
                  + Rule.AllowOrDeny(true);
        var three = Rule.AllowOrDeny(true)
                    & Rule.AllowOrDeny(true);

        IPolicy combined = one | two | three;

        await Assert.That(combined).IsNotNull();
    }

    private static async Task AssertRuleSet(RuleSet combined, Operator expOperator, int expRuleSets, int expRules = 0)
    {
        await Assert.That(combined.Operator).IsEqualTo(expOperator);
        await Assert.That(combined.Rules.Count).IsEqualTo(expRules);
        await Assert.That(combined.RuleSets.Count).IsEqualTo(expRuleSets);
    }

    private static async Task AssertRuleSet(RuleSet combined, Operator expOperator, int expRuleSets, params Rule[] rules)
    {
        await Assert.That(combined.Operator).IsEqualTo(expOperator);
        await Assert.That(combined.RuleSets.Count).IsEqualTo(expRuleSets);
        var index = 0;
        foreach (var rule in rules)
        {
            await Assert.That(combined.Rules[index]).IsEqualTo(rule);
            index++;
        }
    }
}