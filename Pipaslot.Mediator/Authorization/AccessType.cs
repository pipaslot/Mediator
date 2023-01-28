namespace Pipaslot.Mediator.Authorization
{
    /// <summary>
    /// Final access type calculated from all <see cref="RuleOutcome"/> provided by <see cref="RuleSet"/>
    /// </summary>
    public enum AccessType
    {
        /// <summary>
        /// Default state applied in case that no rule was defined or some permit was not allowed or all are ignored.
        /// </summary>
        Deny = 0,
        /// <summary>
        /// All Rule outcomes are allowed or ignored. But it has to have at least one allowed if all other are ignored
        /// </summary>
        Allow = 1,
        /// <summary>
        /// Any Rule outcome is unavailable.
        /// </summary>
        Unavailable = 2,
    }
}
