using System;
using System.Linq;

namespace Pipaslot.Mediator.Authorization.RuleSetFormatters
{
    public class ExceptionRuleSetFormatter : IExceptionRuleSetFormatter
    {
        public string Format(RuleSet set)
        {
            return $"Policy rules: {Format(set)} not matched for current user.";
        }

        internal string FormatInternal(RuleSet set)
        {

            var notGrantedSets = set.RuleSets
                .Where(r => r.GetRuleOutcome().IsNotGranted())
                .Select(r => FormatInternal(r));

            var notGrantedRules = set.Rules
                .Where(r => r.Outcome.IsNotGranted())
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
    }
}
