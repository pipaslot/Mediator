namespace Pipaslot.Mediator.Authorization;

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