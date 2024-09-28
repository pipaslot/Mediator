namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizationRuleNotMetException : AuthorizationException
    {
        public AuthorizationRuleNotMetException(RuleSet ruleSet, AuthorizationExceptionTypes type, string message) : base(type, ProvideDefaultMessage(message))
        {
            RuleSet = ruleSet;
        }
        public AuthorizationRuleNotMetException(RuleSet ruleSet, string message) : this(ruleSet,AuthorizationExceptionTypes.RuleNotMet, ProvideDefaultMessage(message))
        {
            RuleSet = ruleSet;
        }

        public RuleSet RuleSet { get; }

        private static string ProvideDefaultMessage(string message)
        {
            return !string.IsNullOrWhiteSpace(message) ? message : "Policy rules not matched for the current user";
        }
    }
}
