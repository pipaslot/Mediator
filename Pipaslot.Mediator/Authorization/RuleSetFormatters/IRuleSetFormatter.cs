namespace Pipaslot.Mediator.Authorization.RuleSetFormatters
{
    /// <summary>
    /// Strategy definition for converting rules and their rule sets to string
    /// </summary>
    public interface IRuleSetFormatter
    {
        string Format(RuleSet set);
    }
}
