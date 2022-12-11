namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizationRuleException : AuthorizationException
    {
        public AuthorizationRuleException(AuthorizationExceptionTypes type, string message) : base(type, message)
        {
        }

        public RuleSet? RuleSet { get; set; }

        internal static AuthorizationRuleException Create(RuleSet ruleSet, string message)
        {
            var ex = new AuthorizationRuleException(AuthorizationExceptionTypes.RuleNotMet, message);
            ex.RuleSet = ruleSet;
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
