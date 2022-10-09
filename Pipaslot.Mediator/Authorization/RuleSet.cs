using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization
{

    /// <summary>
    /// Define one or more rules aggregated with AND or OR operator
    /// By wrapping two RuleSets in parent RuleSet you can define condition like: ( ( Rule1 OR Rule2 ) AND ( Rule3 OR Rule4 ) )
    /// </summary>
    public class RuleSet
    {
        public Operator Operator { get; }

        public List<Rule> Rules { get; set; } = new List<Rule>();
        public List<RuleSet> RuleSets { get; set; } = new List<RuleSet>();

        /// <summary>
        /// Iterate through all rules and rule sets
        /// </summary>
        public IEnumerable<Rule> RulesRecursive => Rules.Concat(RuleSets.SelectMany(s => s.RulesRecursive));

        public RuleSet(Operator @operator = Operator.And)
        {
            Operator = @operator;
        }
        public RuleSet(params RuleSet[] sets) : this(Operator.And, sets)
        {
        }
        public RuleSet(ICollection<RuleSet> sets) : this(Operator.And, sets)
        {
        }

        public RuleSet(Operator @operator, ICollection<RuleSet> sets)
        {
            Operator = @operator;
            RuleSets.AddRange(sets);
        }
        public RuleSet(params Rule[] rules) : this(Operator.And, rules)
        {
        }

        public RuleSet(ICollection<Rule> rules) : this(Operator.And, rules)
        {
        }

        public RuleSet(Operator @operator, ICollection<Rule> rules)
        {
            Operator = @operator;
            Rules.AddRange(rules);
        }

        public static RuleSet Create(Operator @operator, params Rule[] set)
        {
            return new RuleSet(@operator, set);
        }

        public static RuleSet Create(Operator @operator, params RuleSet[] set)
        {
            return new RuleSet(@operator, set);
        }

        public string StringifyNotGranted()
        {
            var notGrantedSets = RuleSets
                .Where(r => !r.IsGranted())
                .Select(r => r.StringifyNotGranted());

            var notGrantedRules = Rules
                .Where(r => !r.Granted)
                .GroupBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(FormatGroup);
            var notGranted = notGrantedSets.Concat(notGrantedRules)
                .ToArray();

            if (notGranted.Length == 1)
            {
                return notGranted.First();
            }
            return $"({string.Join($" {Operator} ", notGranted)})";
        }

        private string FormatGroup(IGrouping<string, Rule> group)
        {
            return group.Count() > 1
            ? $"{{'{group.Key}': [{string.Join($" {Operator} ", group.Select(r => $"'{r.Value}'"))}]}}"
            : $"{{'{group.Key}': '{group.FirstOrDefault()?.Value}'}}";
        }

        public bool IsGranted()
        {
            var granted = Operator == Operator.And;
            foreach (var rule in RulesRecursive)
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
    }
}
