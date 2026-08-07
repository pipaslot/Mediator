namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Thrown by the authorization middleware when the current user did not satisfy the action's policy.
/// </summary>
/// <remarks>
/// The expected outcome of a denied action, not a configuration problem - a missing or inconsistent policy raises the
/// plain <see cref="AuthorizationException"/> instead. <see cref="RuleSet"/> holds the evaluated tree, so the reason can
/// be rendered for the user instead of a bare "access denied"; the message already carries that rendering.
/// </remarks>
public class AuthorizationRuleNotMetException(RuleSet ruleSet, AuthorizationExceptionTypes type, string message) : AuthorizationException(type,
    ProvideDefaultMessage(message))
{
    public AuthorizationRuleNotMetException(RuleSet ruleSet, string message) : this(ruleSet, AuthorizationExceptionTypes.RuleNotMet,
        ProvideDefaultMessage(message))
    {
        RuleSet = ruleSet;
    }

    public RuleSet RuleSet { get; } = ruleSet;

    private static string ProvideDefaultMessage(string message)
    {
        return !string.IsNullOrWhiteSpace(message) ? message : "Policy rules not matched for the current user";
    }
}