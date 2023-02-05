using System;
using System.Data;
using System.Linq;
using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization.Formatting
{
    public class DefaultRuleFormatter : IRuleFormatter
    {
        public virtual IRule FormatMultiple(IRule[] rules, RuleOutcome outcome, Operator @operator)
        {
            var notEmpty = rules
                .Where(r=>!string.IsNullOrWhiteSpace(r.Value))
                .ToArray();
            if(notEmpty.Length == 0)
            {
                return new Rule(outcome, string.Empty);
            }
            if (notEmpty.Length == 1)
            {
                return FormatSingle(notEmpty.First(), outcome, false);
            }
            var operation = FormatOperator(@operator);
            var sets = rules
                .Select(g => FormatSingle(g, outcome, true))
                .Select(g => g.Value)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToArray();
            var joined = $"{string.Join(operation, sets)}";
            return new Rule(Rule.JoinedFormatedRuleName, joined);
        }

        public virtual IRule FormatSingle(IRule rule, RuleOutcome outcome)
        {
            return FormatSingle(rule, outcome, false);
        }

        protected virtual IRule FormatSingle(IRule rule, RuleOutcome outcome, bool fromMultiple)
        {
            if (rule.Name == IdentityPolicy.AuthenticationPolicyName)
            {
                if (outcome == RuleOutcome.Allow)
                {
                    return new Rule(Rule.DefaultName, string.Empty);
                }
                if (rule.Value == IdentityPolicy.AuthenticatedValue)
                {
                    return new Rule(Rule.DefaultName, FormatRequiredAuthentication());
                }
            }
            if (rule.Name == ClaimTypes.Role)
            {
                return new Rule(Rule.DefaultName, FormatRole(rule));
            }
            if (rule.Name == Rule.DefaultName)
            {
                return rule;
            }
            if (rule.Name == Rule.JoinedFormatedRuleName)
            {
                return string.IsNullOrWhiteSpace(rule.Value) || !fromMultiple
                    ? rule
                    : new Rule(Rule.DefaultName, WrapMultipleRules(rule));
            }
            if (outcome == RuleOutcome.Allow)
            {
                return new Rule(outcome, string.Empty);
            }

            return new Rule(Rule.DefaultName, FormatDefault(rule));
        }

        protected virtual string FormatDefault(IRule rule)
        {
            return $"{rule.Name} '{rule.Value}' is required.";
        }

        protected virtual string WrapMultipleRules(IRule rule)
        {
            return $"({rule.Value})";
        }

        protected virtual string FormatRequiredAuthentication()
        {
            return "User has to be authenticated.";
        }

        protected virtual string FormatRole(IRule rule)
        {
            return $"Role '{rule.Value}' is required.";
        }

        protected virtual string FormatOperator(Operator @operator)
        {
            return @operator == Operator.And 
                ? " AND " 
                : @operator == Operator.Or 
                    ? " OR " 
                    : " ";
        }
    }
}
