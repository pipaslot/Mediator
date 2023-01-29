namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizationRuleNotMetException : AuthorizationException
    {
        public AuthorizationRuleNotMetException(RuleSet ruleSet, AuthorizationExceptionTypes type, string message) : base(type, message)
        {
            RuleSet = ruleSet;
        }

        public RuleSet RuleSet { get; }

        internal static AuthorizationRuleNotMetException Create(RuleSet ruleSet, string message)
        {
            var fullMessage = $"Policy rules not matched for the current user: {message}";
            return new AuthorizationRuleNotMetException(ruleSet, AuthorizationExceptionTypes.RuleNotMet, fullMessage);
        }
    }
}
