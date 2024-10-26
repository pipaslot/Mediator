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

public static class RuleOutcomeExtensions
{
    internal static bool IsAllowedOrIgnored(this RuleOutcome outcome)
    {
        return outcome == RuleOutcome.Allow || outcome == RuleOutcome.Ignored;
    }

    internal static bool IsNotGranted(this RuleOutcome outcome)
    {
        return outcome == RuleOutcome.Deny || outcome == RuleOutcome.Unavailable;
    }

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