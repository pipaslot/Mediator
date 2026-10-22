namespace Pipaslot.Mediator.Authorization;

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

/// <summary>
/// Conversion of the internal <see cref="RuleOutcome"/> to the <see cref="AccessType"/> reported to callers.
/// </summary>
public static class RuleOutcomeExtensions
{
    internal static bool IsAllowedOrIgnored(this RuleOutcome outcome)
    {
        return outcome is RuleOutcome.Allow or RuleOutcome.Ignored;
    }

    internal static bool IsNotGranted(this RuleOutcome outcome)
    {
        return outcome is RuleOutcome.Deny or RuleOutcome.Unavailable;
    }

    /// <summary>
    /// <see cref="RuleOutcome.Ignored"/> maps to <see cref="AccessType.Deny"/> - an outcome no rule decided never grants access.
    /// </summary>
    public static AccessType ToAccessType(this RuleOutcome outcome)
    {
        return outcome switch
        {
            RuleOutcome.Allow => AccessType.Allow,
            RuleOutcome.Unavailable => AccessType.Unavailable,
            _ => AccessType.Deny
        };
    }
}