using Pipaslot.Mediator.Authorization;
using System.Data.Common;
using System.Linq;

namespace Pipaslot.Mediator.Tests.Authorization;

public class RuleSet_OperatorTests
{
    [Fact]
    public void ADD_ThreeTimeTheSame()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1 + r2 + r3;
        AssertRuleSet(combined, Operator.Add, 1, r3);
        var subSet = combined.RuleSets.First();
        AssertRuleSet(subSet, Operator.Add, 0, r1, r2);
    }

    [Fact]
    public void AND_ThreeTimeTheSame()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1 & r2 & r3;
        AssertRuleSet(combined, Operator.And, 1, r3);
        var subSet = combined.RuleSets.First();
        AssertRuleSet(subSet, Operator.And, 0, r1, r2);
    }

    [Fact]
    public void OR_ThreeTimeTheSame()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1 | r2 | r3;
        AssertRuleSet(combined, Operator.Or, 1, r3);
        var subSet = combined.RuleSets.First();
        AssertRuleSet(subSet, Operator.Or, 0, r1, r2);
    }

    [Fact]
    public void ORADDWithBrackets_RuleSetConbinedWitRuleByOrAndWithInverted_AppendTheRule()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = (r1
                        | r2)
                       + r3;
        AssertRuleSet(combined, Operator.Add, 1, r3);
        var subSet = combined.RuleSets.First();
        AssertRuleSet(subSet, Operator.Or, 0, r1, r2);
    }


    [Fact]
    public void ORANDWithBrackets_RuleSetConbinedWitRuleByOrAndWithInverted_AppendTheRule()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = (r1
                        | r2)
                       & r3;
        AssertRuleSet(combined, Operator.And, 1, r3);
        var subSet = combined.RuleSets.First();
        AssertRuleSet(subSet, Operator.Or, 0, r1, r2);
    }

    [Fact]
    public void ORADD_RuleSetConbinedWitRuleByOrAndWithInverted_AppendTheRule()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1
                       | (r2
                          + r3);
        AssertRuleSet(combined, Operator.Or, 1, r1);
        var subSet = combined.RuleSets.First();
        AssertRuleSet(subSet, Operator.Add, 0, r2, r3);
    }

    [Fact]
    public void ORAND_RuleSetConbinedWitRuleByOrAndWithInverted_AppendTheRule()
    {
        var r1 = Rule.AllowOrDeny(true, "", "R1");
        var r2 = Rule.AllowOrDeny(true, "", "R2");
        var r3 = Rule.AllowOrDeny(true, "", "R3");
        var combined = r1
                       | (r2
                          & r3);
        AssertRuleSet(combined, Operator.Or, 1, r1);
        var subSet = combined.RuleSets.First();
        AssertRuleSet(subSet, Operator.And, 0, r2, r3);
    }

    [Fact]
    public void CastCombinedRulesToPolicy()
    {
        IPolicy combined = Rule.AllowOrDeny(true)
                           | ((Rule.AllowOrDeny(true)
                               + Rule.AllowOrDeny(true))
                              & Rule.AllowOrDeny(true))
                           | Rule.AllowOrDeny(true);

        Assert.NotNull(combined);
    }

    [Fact]
    public void CastCombinedRuleSetsToPolicy()
    {
        var one = Rule.AllowOrDeny(true)
                  | Rule.AllowOrDeny(true);
        var two = Rule.AllowOrDeny(true)
                  + Rule.AllowOrDeny(true);
        var three = Rule.AllowOrDeny(true)
                    & Rule.AllowOrDeny(true);

        IPolicy combined = one | two | three;

        Assert.NotNull(combined);
    }

    private static void AssertRuleSet(RuleSet combined, Operator expOperator, int expRuleSets, int expRules = 0)
    {
        Assert.Equal(expOperator, combined.Operator);
        Assert.Equal(expRules, combined.Rules.Count);
        Assert.Equal(expRuleSets, combined.RuleSets.Count);
    }

    private static void AssertRuleSet(RuleSet combined, Operator expOperator, int expRuleSets, params Rule[] rules)
    {
        Assert.Equal(expOperator, combined.Operator);
        Assert.Equal(expRuleSets, combined.RuleSets.Count);
        var index = 0;
        foreach (var rule in rules)
        {
            Assert.Equal(rule, combined.Rules[index]);
            index++;
        }
    }
}