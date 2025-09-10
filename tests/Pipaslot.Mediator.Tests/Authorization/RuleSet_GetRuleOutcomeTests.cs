using Pipaslot.Mediator.Authorization;
using System.Linq;

namespace Pipaslot.Mediator.Tests.Authorization;

public class RuleSet_GetRuleOutcomeTests
{
    [Test]
    // Empty 
    [Arguments(new RuleOutcome[] { }, Operator.Add, RuleOutcome.Ignored)]
    [Arguments(new RuleOutcome[] { }, Operator.And, RuleOutcome.Deny)]
    [Arguments(new RuleOutcome[] { }, Operator.Or, RuleOutcome.Ignored)]

    // Ignored only
    [Arguments(new[] { RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Ignored)]
    [Arguments(new[] { RuleOutcome.Ignored }, Operator.And, RuleOutcome.Deny)]
    [Arguments(new[] { RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Ignored)]
    // Ignored together with other
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Allow }, Operator.Add, RuleOutcome.Allow)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Allow }, Operator.And, RuleOutcome.Deny)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Allow }, Operator.Or, RuleOutcome.Allow)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Deny }, Operator.Add, RuleOutcome.Deny)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Deny }, Operator.And, RuleOutcome.Deny)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Deny }, Operator.Or, RuleOutcome.Deny)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Unavailable }, Operator.Add, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Unavailable }, Operator.And, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Unavailable }, Operator.Or, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Ignored)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Deny)]
    [Arguments(new[] { RuleOutcome.Ignored, RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Ignored)]

    // Unavailable only
    [Arguments(new[] { RuleOutcome.Unavailable }, Operator.Add, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable }, Operator.And, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable }, Operator.Or, RuleOutcome.Unavailable)]
    // Unavailable together with other
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Allow)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Allow)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.Or,
        RuleOutcome.Deny)] // Return the value more close to alowed status
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Unavailable)]
    [Arguments(new[] { RuleOutcome.Unavailable, RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Unavailable)]

    // Allow and Deny together
    [Arguments(new[] { RuleOutcome.Allow, RuleOutcome.Deny }, Operator.Add, RuleOutcome.Deny)]
    [Arguments(new[] { RuleOutcome.Allow, RuleOutcome.Deny }, Operator.And, RuleOutcome.Deny)]
    [Arguments(new[] { RuleOutcome.Allow, RuleOutcome.Deny }, Operator.Or, RuleOutcome.Allow)]
    public async Task GetRuleOutcome_Combinations(RuleOutcome[] outcomes, Operator @operator, RuleOutcome expected)
    {
        var sut = new RuleSet(@operator);
        sut.Rules.AddRange(outcomes.Select(o => new Rule(o, string.Empty)));

        var result = sut.Reduce();
        await Assert.That(result.Outcome).IsEqualTo(expected);
    }
}