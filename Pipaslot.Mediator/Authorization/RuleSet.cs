using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization
{

    /// <summary>
    /// Define one or more rules aggregated with AND or OR operator
    /// </summary>
    public class RuleSet : IRuleSet, IEnumerable<Rule>
    {
        public Operator Operator { get; }
        public bool Granted => IsGranted();

        private List<Rule> _rules = new List<Rule>();

        public RuleSet(params Rule[] rules) : this(Operator.And, rules)
        {
        }

        public RuleSet(Operator @operator, params Rule[] rules)
        {
            Operator = @operator;
            _rules.AddRange(rules);
        }

        public string StringifyNotGranted()
        {
            var notGrantedGroups = _rules
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
            var granted = Operator == Operator.And;
            foreach (var rule in _rules)
            {
                if (Operator == Operator.And)
                {
                    granted &= rule.Granted;
                }
                else if (Operator == Operator.Or)
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

        public IEnumerator<Rule> GetEnumerator()
        {
            return _rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rules.GetEnumerator();
        }
    }
}
