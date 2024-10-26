namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Operator applied to RuleSet. Determine how the final RuleOutcome will be calculated
/// </summary>
public enum Operator
{
    /// <summary>
    /// Final result will be Allowed when all rules or sub-rulesets will be allowed or Ignored. The calculation skips ignored rules.
    /// </summary>
    Add,

    /// <summary>
    /// Final result will be Allowed when all rules or sub-rulesets will be allowed. The calculation do not accept ignored rules.
    /// </summary>
    And,

    /// <summary>
    /// Final result will be Allowed when at least one rule or sub-ruleset will be allowed.
    /// </summary>
    Or
}