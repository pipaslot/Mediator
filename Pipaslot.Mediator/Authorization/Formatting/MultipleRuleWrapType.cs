namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// Control wrapping of rule sets having multiple rules with another rule set. Used for brackets application when for cases like "(Rule1 AND Rule2) OR Rule3"
/// </summary>
public enum MultipleRuleWrapType
{
    /// <summary>
    /// Rules will always be wrapped when combining multiple rule sets
    /// Example: "Rule1FromRuleset1 OR Rule2FromRuleset1 OR Rule3FromRuleSet2 OR Rule4FromRuleSet3 AND Rule4FromRuleSet3" will be converted
    /// to "(Rule1FromRuleset1 OR Rule2FromRuleset1) OR Rule3FromRuleSet2 OR (Rule4FromRuleSet3 AND Rule4FromRuleSet3)"
    /// </summary>
    Always,
    
    /// <summary>
    /// Rules will always be wrapped when combining multiple rule sets
    /// Example: "Rule1FromRuleset1 OR Rule2FromRuleset1 OR Rule3FromRuleSet2 OR Rule4FromRuleSet3 AND Rule4FromRuleSet3" will be converted
    /// to "Rule1FromRuleset1 OR Rule2FromRuleset1 OR Rule3FromRuleSet2 OR (Rule4FromRuleSet3 AND Rule4FromRuleSet3)"
    /// </summary>
    DifferentOperator,
    
    /// <summary>
    /// No wrapping will be applied
    /// </summary>
    Never
}