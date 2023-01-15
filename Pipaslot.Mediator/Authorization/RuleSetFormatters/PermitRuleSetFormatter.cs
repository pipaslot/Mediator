using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization.RuleSetFormatters
{
    public class PermitRuleSetFormatter : IAllowedRuleSetFormatter, IDeniedRuleSetFormatter
    {
        public string Format(RuleSet set)
        {
            var isGranted = set.GetRuleOutcome().ToAccessType() == AccessType.Allow;
            var messages = CollectReasons(set, isGranted)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(" ", messages);
        }

        private IEnumerable<string> CollectReasons(RuleSet set, bool isFinallyGranted)
        {
            foreach (var r in set.RuleSets
                .Where(r => isFinallyGranted == r.GetRuleOutcome().IsAllowedOrIgnored()))
            {
                foreach (var child in CollectReasons(r, isFinallyGranted))
                {
                    yield return child;
                }
            }
            foreach (var r in set.Rules
                .Where(r => isFinallyGranted == r.Outcome.IsAllowedOrIgnored()))
            {
                yield return FormatRule(r);
            }
        }

        protected virtual string FormatRule(Rule rule)
        {
            if (rule.Outcome == RuleOutcome.Ignored)
            {
                return string.Empty;
            }
            if (rule.Name == IdentityPolicy.AuthenticationPolicyName)
            {
                if (rule.Outcome == RuleOutcome.Allow)
                {
                    return string.Empty;
                }
                if (rule.Value == IdentityPolicy.AuthenticatedValue)
                {
                    return "User has to be authenticated.";
                }
            }
            if (rule.Name == ClaimTypes.Role)
            {
                if (rule.Outcome == RuleOutcome.Allow)
                {
                    return string.Empty;
                }
                return $"Role '{rule.Value}' is required.";
            }
            if (rule.Name == Rule.DefaultName)
            {
                return rule.Value;
            }

            return $"{rule.Name} '{rule.Value}' is required.";
        }
    }
}