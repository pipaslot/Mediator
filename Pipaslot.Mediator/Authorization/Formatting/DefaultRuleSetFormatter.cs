using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Pipaslot.Mediator.Authorization.Formatting
{
    public class DefaultRuleSetFormatter : IRuleSetFormatter
    {
        public IRule FormatSingle(IRule rule, RuleOutcome outcome)
        {
            if (rule.Name == IdentityPolicy.AuthenticationPolicyName)
            {
                if (outcome == RuleOutcome.Allow)
                {
                    return new Rule(Rule.DefaultName, string.Empty);
                }
                if (rule.Value == IdentityPolicy.AuthenticatedValue)
                {
                    return new Rule(Rule.DefaultName, "User has to be authenticated.");
                }
            }
            if (rule.Name == ClaimTypes.Role)
            {
                return new Rule(Rule.DefaultName, $"Role '{rule.Value}' is required.");
            }
            if (rule.Name == Rule.DefaultName /*|| rule.Name == Rule.JoinedFormatedRuleName*/)
            {
                return rule;
            }
            if (outcome == RuleOutcome.Allow)
            {
                return new Rule(outcome, string.Empty);
            }

            return new Rule(Rule.DefaultName, $"{rule.Name} '{rule.Value}' is required.");
        }

        public IRule FormatMultiple(IRule[] rules, RuleOutcome outcome, Operator @operator)
        {
            return Join(rules, @operator == Operator.And ? "AND" : "OR");
        }

        protected IRule Join(IRule[] denied, string operation)
        {
            var sets = denied
                            .GroupBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase)
                            .Select(g => FormatGroup(g, operation))
                            .Where(r => !string.IsNullOrWhiteSpace(r))
                            .ToArray();
            if (sets.Length == 1)
            {
                return new Rule(Rule.DefaultName, sets.First());
            }
            return new Rule(Rule.JoinedFormatedRuleName, $"{string.Join($" {operation} ", sets)}");
        }

        protected string FormatGroup(IGrouping<string, IRule> group, string op)
        {
            var nonEmptyRules = group
                .Where(g => !string.IsNullOrWhiteSpace(g.Value))
                .ToArray();
            if (nonEmptyRules.Length == 0)
            {
                return string.Empty;
            }

            if (group.Key == Rule.DefaultName || group.Key == Rule.JoinedFormatedRuleName)
            {
                return string.Join($" {op} ",
                    nonEmptyRules
                    .Select(r => r.Name == Rule.JoinedFormatedRuleName
                        ? $"({r.Value})"
                        : r.Value));
            }
            return nonEmptyRules.Count() > 1
            ? $"{{'{group.Key}': [{string.Join($" {op} ", nonEmptyRules.Select(r => $"'{r.Value}'"))}]}}"
            : $"{{'{group.Key}': '{nonEmptyRules.FirstOrDefault()?.Value}'}}";
        }
    }
}
