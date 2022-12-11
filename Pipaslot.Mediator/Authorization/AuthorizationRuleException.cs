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
            var ex = new AuthorizationRuleException(ruleSet, AuthorizationExceptionTypes.RuleNotMet, message);
            var i = 1;
            foreach (var rule in ruleSet.RulesRecursive)
            {
                ex.Data[i] = rule;
                i++;
            }
            return ex;
        }
    }
}
