using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using System.Linq;

namespace Pipaslot.Mediator.Tests.Authorization;

public class RuleSet_GetRuleOutcomeTests
{
    [Theory]
    // Empty 
    [InlineData(new RuleOutcome[] { }, Operator.Add, RuleOutcome.Ignored)]
    [InlineData(new RuleOutcome[] { }, Operator.And, RuleOutcome.Deny)]
    [InlineData(new RuleOutcome[] { }, Operator.Or, RuleOutcome.Ignored)]

    // Ignored only
    [InlineData(new[] { RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Ignored)]
    [InlineData(new[] { RuleOutcome.Ignored }, Operator.And, RuleOutcome.Deny)]
    [InlineData(new[] { RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Ignored)]
    // Ignored together with other
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Allow }, Operator.Add, RuleOutcome.Allow)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Allow }, Operator.And, RuleOutcome.Deny)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Allow }, Operator.Or, RuleOutcome.Allow)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Deny }, Operator.Add, RuleOutcome.Deny)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Deny }, Operator.And, RuleOutcome.Deny)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Deny }, Operator.Or, RuleOutcome.Deny)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Unavailable }, Operator.Add, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Unavailable }, Operator.And, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Unavailable }, Operator.Or, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Ignored)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Deny)]
    [InlineData(new[] { RuleOutcome.Ignored, RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Ignored)]

    // Unavailable only
    [InlineData(new[] { RuleOutcome.Unavailable }, Operator.Add, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable }, Operator.And, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable }, Operator.Or, RuleOutcome.Unavailable)]
    // Unavailable together with other
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Allow)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Allow, RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Allow)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Deny, RuleOutcome.Ignored }, Operator.Or,
        RuleOutcome.Deny)] // Return the value more close to alowed status
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Ignored }, Operator.Add, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Ignored }, Operator.And, RuleOutcome.Unavailable)]
    [InlineData(new[] { RuleOutcome.Unavailable, RuleOutcome.Ignored }, Operator.Or, RuleOutcome.Unavailable)]

    // Allow and Deny together
    [InlineData(new[] { RuleOutcome.Allow, RuleOutcome.Deny }, Operator.Add, RuleOutcome.Deny)]
    [InlineData(new[] { RuleOutcome.Allow, RuleOutcome.Deny }, Operator.And, RuleOutcome.Deny)]
    [InlineData(new[] { RuleOutcome.Allow, RuleOutcome.Deny }, Operator.Or, RuleOutcome.Allow)]
    public void GetRuleOutcome_Combinations(RuleOutcome[] outcomes, Operator @operator, RuleOutcome expected)
    {
        var sut = new RuleSet(@operator);
        sut.Rules.AddRange(outcomes.Select(o => new Rule(o, string.Empty)));

        var result = sut.Reduce();
        Assert.Equal(expected, result.Outcome);
    }
}