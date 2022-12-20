using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace Pipaslot.Mediator.Authorization
{
    public class RuleSetFormatter : IRuleSetFormatter
    {
        /// <summary>
        /// Provide a Singleton instance
        /// </summary>
        private static RuleSetFormatter? _instance;

        private RuleSetFormatter()
        {
        }

        public static RuleSetFormatter Instance => _instance ??= new RuleSetFormatter();

        public string FormatException(RuleSet set)
        {
            return $"Policy rules: {Format(set)} not matched for current user.";
        }

        internal string Format(RuleSet set)
        {
            var notGrantedSets = set.RuleSets
                .Where(r => !r.IsGranted())
                .Select(r => Format(r));

            var notGrantedRules = set.Rules
                .Where(r => !r.Granted)
                .GroupBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(g => FormatGroup(g, set.Operator));
            var notGranted = notGrantedSets.Concat(notGrantedRules)
                .ToArray();

            if (notGranted.Length == 0)
            {
                return string.Empty;
            }
            if (notGranted.Length == 1)
            {
                return notGranted.First();
            }
            return $"({string.Join($" {set.Operator} ", notGranted)})";
        }

        private string FormatGroup(IGrouping<string, Rule> group, Operator op)
        {
            return group.Count() > 1
            ? $"{{'{group.Key}': [{string.Join($" {op} ", group.Select(r => $"'{r.Value}'"))}]}}"
            : $"{{'{group.Key}': '{group.FirstOrDefault()?.Value}'}}";
        }

        public string FormatReason(RuleSet set)
        {
            var isGranted = set.IsGranted();
            var messages = CollectReasons(set, isGranted)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(" ", messages);
        }

        private IEnumerable<string> CollectReasons(RuleSet set, bool isFinallyGranted)
        {
            foreach (var r in set.RuleSets
                .Where(r => isFinallyGranted == r.IsGranted()))
            {
                foreach (var child in CollectReasons(r, isFinallyGranted))
                {
                    yield return child;
                }
            }
            foreach (var r in set.Rules
                .Where(r => isFinallyGranted == r.Granted))
            {
                yield return FormatRule(r);
            }
        }

        protected virtual string FormatRule(Rule rule)
        {
            if (rule.Name == IdentityPolicy.AuthenticationPolicyName)
            {
                if (rule.Granted)
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
                if (rule.Granted)
                {
                    return string.Empty;
                }
                return $"Role {rule.Value} is required.";
            }
            if (rule.Name == Rule.DefaultName)
            {
                return rule.Value;
            }

            return $"{rule.Name} {rule.Value} is required.";
        }
    }
}
