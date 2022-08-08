using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization
{

    /// <summary>
    /// Define one or more rules aggregated with AND or OR operator
    /// </summary>
    public class RuleSet : List<Rule>, IRuleSet
    {
        public RuleOperator Operator { get; }
        public bool Granted => IsGranted();

        public RuleSet(params Rule[] rules) : base(rules)
        {
            Operator = RuleOperator.And;
        }

        public RuleSet(RuleOperator @operator, params Rule[] rules) : base(rules)
        {
            Operator = @operator;
        }

        public string StringifyNotGranted()
        {
            var notGrantedGroups = this
                .Where(r => !r.Granted)
                .GroupBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(FormatGroup)
                .ToArray();
            if (notGrantedGroups.Length == 1)
            {
                return notGrantedGroups.First();
            }
            return $"({string.Join($" {Operator} ", notGrantedGroups)})";
        }

        private string FormatGroup(IGrouping<string, Rule> group)
        {
            return group.Count() > 1
            ? $"{{'{group.Key}': [{string.Join($" {Operator} ", group.Select(r => $"'{r.Value}'"))}]}}"
            : $"{{'{group.Key}': '{group.FirstOrDefault()?.Value}'}}";
        }

        private bool IsGranted()
        {
            var granted = Operator == RuleOperator.And;
            foreach (var rule in this)
            {
                if (Operator == RuleOperator.And)
                {
                    granted &= rule.Granted;
                }
                else if (Operator == RuleOperator.Or)
                {
                    granted |= rule.Granted;
                }
                else
                {
                    throw new NotImplementedException($"Unknown operator '{Operator}'");
                }
            }
            return granted;
        }
    }
}
