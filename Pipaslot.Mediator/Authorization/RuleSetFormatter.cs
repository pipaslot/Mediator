using System;
using System.Data;
using System.Linq;

namespace Pipaslot.Mediator.Authorization
{
    public class RuleSetFormatter
    {
        /// <summary>
        /// Provide a Singleto ninstance
        /// </summary>
        private static RuleSetFormatter? _instance;

        public RuleSetFormatter()
        {
        }

        public static RuleSetFormatter Instance => _instance ??= new RuleSetFormatter();


        public string Format(RuleSet set)
        {
            var notGrantedSets = set.RuleSets
                .Where(r => !r.IsGranted())
                .Select(r => Format(r));

            var notGrantedRules = set.Rules
                .Where(r => !r.Granted)
                .GroupBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(g=>FormatGroup(g ,set.Operator));
            var notGranted = notGrantedSets.Concat(notGrantedRules)
                .ToArray();

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
