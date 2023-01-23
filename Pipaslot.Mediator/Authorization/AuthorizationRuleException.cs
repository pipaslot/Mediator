namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizationRuleException : AuthorizationException
    {
        public AuthorizationRuleException(RuleSet ruleSet, AuthorizationExceptionTypes type, string message) : base(type, message)
        {
            RuleSet = ruleSet;
        }

        public RuleSet RuleSet { get; }

        internal static AuthorizationRuleException Create(RuleSet ruleSet, string message)
        {
            var fullMessage = $"Policy rules not matched for the current user: {message}";
            return new AuthorizationRuleException(ruleSet, AuthorizationExceptionTypes.RuleNotMet, fullMessage);
        }
    }
}
