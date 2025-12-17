using Pipaslot.Mediator.Authorization;

namespace Pipaslot.Mediator.Tests.Authorization;

public class RuleTests
{
    [Fact]
    public void Default_IsReducedToDeny()
    {
        AssertAccessType(Rule.Default, AccessType.Deny);
    }
    
    [Theory]
    [InlineData(RuleOutcome.Allow, AccessType.Deny)]
    [InlineData(RuleOutcome.Deny, AccessType.Deny)]
    [InlineData(RuleOutcome.Unavailable, AccessType.Unavailable)]
    [InlineData(RuleOutcome.Ignored, AccessType.Deny)]
    public void Default_CombinedWithAnd(RuleOutcome outcome, AccessType expected)
    {
        var set = Rule.Default & new Rule(outcome, string.Empty);
        AssertAccessType(set, expected);
    }
    
    [Theory]
    [InlineData(RuleOutcome.Allow, AccessType.Allow)]
    [InlineData(RuleOutcome.Deny, AccessType.Deny)]
    [InlineData(RuleOutcome.Unavailable, AccessType.Unavailable)]
    [InlineData(RuleOutcome.Ignored, AccessType.Deny)]
    public void Default_CombinedWithAdd(RuleOutcome outcome, AccessType expected)
    {
        var set = Rule.Default + new Rule(outcome, string.Empty);
        AssertAccessType(set, expected);
    }
    
    [Theory]
    [InlineData(RuleOutcome.Allow, AccessType.Allow)]
    [InlineData(RuleOutcome.Deny, AccessType.Deny)]
    [InlineData(RuleOutcome.Unavailable, AccessType.Unavailable)]
    [InlineData(RuleOutcome.Ignored, AccessType.Deny)]
    public void Default_CombinedWithOr(RuleOutcome outcome, AccessType expected)
    {
        var set = Rule.Default | new Rule(outcome, string.Empty);
        AssertAccessType(set, expected);
    }

    private void AssertAccessType(RuleSet set, AccessType expected)
    {
        var node = set.Reduce();
        var accessType = node.Outcome.ToAccessType();
        Assert.Equal(expected, accessType);
    }
}