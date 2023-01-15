namespace Pipaslot.Mediator.Authorization
{
    /// <summary>
    /// Rule outcome is used for computing final permit
    /// </summary>
    public enum RuleOutcome
    {
        /// <summary>
        /// User can not perform the operation
        /// </summary>
        Deny = 0,
        /// <summary>
        /// User can perform the operation
        /// </summary>
        Allow = 1,
        /// <summary>
        /// The operations shouldn't be available/shown to the user 
        /// </summary>
        Unavailable = 2,
        /// <summary>
        /// The rule should be skipped.
        /// </summary>
        Ignored = 3
    }
}
