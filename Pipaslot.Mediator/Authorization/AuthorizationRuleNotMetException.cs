namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizationRuleNotMetException : AuthorizationException
    {
        public AuthorizationRuleNotMetException(RuleSet ruleSet, AuthorizationExceptionTypes type, string message) : base(type, message)
        {
            RuleSet = ruleSet;
        }
        public AuthorizationRuleNotMetException(RuleSet ruleSet, string message) : this(ruleSet,AuthorizationExceptionTypes.RuleNotMet, message)
        {
            RuleSet = ruleSet;
        }

        public RuleSet RuleSet { get; }
    }
}
