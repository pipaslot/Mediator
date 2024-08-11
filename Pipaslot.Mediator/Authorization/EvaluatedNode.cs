namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Represents <see cref="Rule"/> or reduced <see cref="RuleSet"/> transformed into structure ready for node aggregations and reason formating.
/// </summary>
/// <param name="Kind">Rule name or customized kind produced during formating.</param>
/// <param name="Outcome"></param>
/// <param name="Value">Value or aggregated and formated reason depending on the kind.</param>
public record struct EvaluatedNode(string Kind, RuleOutcome Outcome, string Value)
{

    /// <summary>
    /// Represents node already formated.
    /// This kind was introduced to prevent multiple formatting when only propagating node from leaves to root.
    /// </summary>
    public const string AlreadyFormated = "Formated";
    
    /// <summary>
    /// The node was created as reduction of multiple Rules or RuleSets in the RuleSet
    /// </summary>
    public const string RuleSetKind = "ReducedRuleSet";
    
    /// <summary>
    /// Nodes joined by the RuleSet Operator.
    /// This kind helps to make visual and logical boundaries like brackets in example: "(A AND B) OR C"
    /// </summary>
    public const string JoinedFormatedRuleName = "JoinedByTheRuleSetOperator";

    public FormatedNode ToFormatedNode()
    {
        return new FormatedNode(Kind, Value);
    }
}