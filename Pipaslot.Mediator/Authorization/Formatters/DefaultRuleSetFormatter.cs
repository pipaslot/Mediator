using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Pipaslot.Mediator.Authorization.Formatters
{
    public class DefaultRuleSetFormatter : IRuleSetFormatter
    {
        public Rule Format(Rule rule)
        {
            if (rule.Name == IdentityPolicy.AuthenticationPolicyName)
            {
                if (rule.Outcome == RuleOutcome.Allow)
                {
                    return new Rule(rule.Outcome, string.Empty);
                }
                if (rule.Value == IdentityPolicy.AuthenticatedValue)
                {
                    return new Rule(rule.Outcome, "User has to be authenticated.");
                }
            }
            if (rule.Name == ClaimTypes.Role)
            {
                return new Rule(rule.Outcome, $"Role '{rule.Value}' is required.");
            }
            if (rule.Name == Rule.DefaultName)
            {
                return rule;
            }
            if (rule.Outcome == RuleOutcome.Allow)
            {
                return new Rule(rule.Outcome, string.Empty);
            }

            return new Rule(Rule.DefaultName, $"{rule.Name} '{rule.Value}' is required.", rule.Outcome);
        }

        public Rule FormatDeniedWithAnd(ICollection<Rule> denied)
        {
            var sets = denied
                .GroupBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(g => FormatGroup(g, "AND"))
                .ToArray();
            if(sets.Length == 1)
            {
                return new Rule(RuleOutcome.Deny, sets.First());
            }
            return new Rule(RuleOutcome.Deny, $"({string.Join($" AND ", sets)})");
        }

        public Rule FormatDeniedWithOr(ICollection<Rule> denied)
        {
            var sets = denied
                .GroupBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(g => FormatGroup(g, "OR"))
                .ToArray();
            if (sets.Length == 1)
            {
                return new Rule(RuleOutcome.Deny, sets.First());
            }
            return new Rule(RuleOutcome.Deny, $"({string.Join($" OR ", sets)})");
        }

        private string FormatGroup(IGrouping<string, Rule> group, string op)
        {
            if(group.Key == Rule.DefaultName)
            {
                return group.Count() > 1
                    ? $"({string.Join($" {op} ", group.Select(r => r.Value))})"
                    : $"{group.FirstOrDefault()?.Value}";
            }
            return group.Count() > 1
            ? $"{{'{group.Key}': [{string.Join($" {op} ", group.Select(r => $"'{r.Value}'"))}]}}"
            : $"{{'{group.Key}': '{group.FirstOrDefault()?.Value}'}}";
        }     
    }
}
