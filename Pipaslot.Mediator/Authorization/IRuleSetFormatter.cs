namespace Pipaslot.Mediator.Authorization
{
    public interface IRuleSetFormatter
    {
        /// <summary>
        /// Format message for end user
        /// </summary>
        string FormatReason(RuleSet set);
        /// <summary>
        /// Format message for exception thrown when some rule is not granted
        /// </summary>
        string FormatException(RuleSet set);
    }
}
