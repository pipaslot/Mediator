using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Pipaslot.Mediator.Authorization
{

    /// <summary>
    /// Define one or more rules aggregated with AND or OR operator
    /// By wrapping two RuleSets in parent RuleSet you can define condition like: ( ( Rule1 OR Rule2 ) AND ( Rule3 OR Rule4 ) )
    /// </summary>
    public class RuleSet : IPolicy
    {
        public Operator Operator { get; }
        public RuleSetReproducibility Reproducibility { get; private set; } = RuleSetReproducibility.Dynamic;
        public List<Rule> Rules { get; set; } = new List<Rule>();
        public List<RuleSet> RuleSets { get; set; } = new List<RuleSet>();

        /// <summary>
        /// Iterate through all rules and rule sets
        /// </summary>
        public IEnumerable<Rule> RulesRecursive => Rules.Concat(RuleSets.SelectMany(s => s.RulesRecursive));
        public IEnumerable<RuleSet> RuleSetsRecursive => RuleSets.Concat(RuleSets.SelectMany(s => s.RuleSetsRecursive));

        public RuleSet(Operator @operator = Operator.And)
        {
            Operator = @operator;
        }

        public RuleSet(params RuleSet[] sets) : this(Operator.And, sets)
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

        public RuleSet(Operator @operator, ICollection<Rule> rules)
        {
            Operator = @operator;
            Rules.AddRange(rules);
        }

        /// <summary>
        /// Create rule set with initialized rule
        /// </summary>
        /// <param name="ruleValue"></param>
        /// <param name="isGranted"></param>
        public RuleSet(string ruleValue, bool isGranted = false)
        {
            Rules.Add(new Rule(ruleValue, isGranted));
        }

        public static RuleSet Create(Operator @operator, params Rule[] set)
        {
            return new RuleSet(@operator, set);
        }

        public static RuleSet Create(Operator @operator, params RuleSet[] set)
        {
            return new RuleSet(@operator, set);
        }

        [Obsolete("Use RuleSetFormatter")]
        public string StringifyNotGranted()
        {
            var formatter = RuleSetFormatter.Instance;
            return formatter.Format(this);
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
        /// <summary>
        /// Set identity static reproducibility
        /// </summary>
        /// <returns></returns>
        public RuleSet SetIdentityStatic()
        {
            Reproducibility = RuleSetReproducibility.IdentityStatic;
            return this;
        }

        public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            return Task.FromResult((RuleSet)this);
        }
    }
}
