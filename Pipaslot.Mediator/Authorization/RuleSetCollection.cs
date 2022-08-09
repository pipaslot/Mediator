using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization
{
    /// <summary>
    /// Recursive collection for RuleSet. Aggregates multiple RuleSets with AND or OR operator.
    /// By wrapping two RuleSets in RuleSetColelction you can define condition like: ( ( Rule1 OR Rule2 ) AND ( Rule3 OR Rule4 ) )
    /// </summary>
    public class RuleSetCollection : List<IRuleSet>, IRuleSet
    {
        public Operator Operator { get; }
        public bool Granted => IsGranted();
        public RuleSetCollection(params IRuleSet[] rules) : base(rules)
        {
            Operator = Operator.And;
        }

        public RuleSetCollection(Operator @operator, params IRuleSet[] rules) : base(rules)
        {
            Operator = @operator;
        }

        public string StringifyNotGranted()
        {
            var notGrantedGroups = this
                .Where(r => !r.Granted)
                .Select(r => r.StringifyNotGranted())
                .ToArray();
            if (notGrantedGroups.Length == 1)
            {
                return notGrantedGroups.First();
            }
            return $"({string.Join($" {Operator} ", notGrantedGroups)})";
        }

        private bool IsGranted()
        {
            var granted = Operator == Operator.And;
            foreach (var rule in this)
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
