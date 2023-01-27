using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Pipaslot.Mediator.Authorization
{

    /// <summary>
    /// Provides connection between policies and rules
    /// Define one or more rules aggregated with AND or OR operator.
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

        public static RuleSet Create(Operator @operator, params Rule[] set)
        {
            return new RuleSet(@operator, set);
        }

        public static RuleSet Create(Operator @operator, params RuleSet[] set)
        {
            return new RuleSet(@operator, set);
        }

        public IEvaluatedRule Evaluate(IRuleSetFormatter formatter)
        {
            var rules = Rules
                .Concat(RuleSets.Select(s => s.Evaluate(formatter)));
            if (Operator == Operator.And)
            {
                return ReduceWithAnd(rules, formatter);
            }
            if (Operator == Operator.Or)
            {
                return ReduceWithOr(rules, formatter);
            }
            throw new NotImplementedException($"Unknown operator '{Operator}'");
        }

        private IEvaluatedRule ReduceWithAnd(IEnumerable<IEvaluatedRule> rules, IRuleSetFormatter formatter)
        {
            var denied = new List<IRule>();
            var unavailable = new List<IRule>();
            var allowed = new List<IRule>();
            foreach (var rule in rules)
            {
                var outcome = rule.Outcome;
                if (outcome == RuleOutcome.Ignored)
                {
                    continue;
                }
                if (outcome == RuleOutcome.Unavailable)
                {
                    unavailable.Add(rule);
                }
                if (outcome == RuleOutcome.Deny)
                {
                    denied.Add(rule);
                }
                if (outcome == RuleOutcome.Allow)
                {
                    allowed.Add(rule);
                }
            }
            if (unavailable.Any())
            {
                return formatter.Format(unavailable, RuleOutcome.Unavailable,Operator.Or);
            }
            if (denied.Any())
            {
                return formatter.Format(denied, RuleOutcome.Deny, Operator.And);
            }
            if (allowed.Any())
            {
                return formatter.Format(allowed, RuleOutcome.Allow, Operator.And);
            }
            return new Rule(RuleOutcome.Ignored, string.Empty);
        }

        private IEvaluatedRule ReduceWithOr(IEnumerable<IEvaluatedRule> rules, IRuleSetFormatter formatter)
        {
            var denied = new List<IRule>();
            var unavailable = new List<IRule>();
            var allowed = new List<IRule>();
            foreach (var rule in rules)
            {
                var outcome = rule.Outcome;
                if (outcome == RuleOutcome.Ignored)
                {
                    continue;
                }
                if (outcome == RuleOutcome.Allow)
                {
                    allowed.Add(rule);
                }
                if (outcome == RuleOutcome.Unavailable)
                {
                    unavailable.Add(rule);
                }
                if (outcome == RuleOutcome.Deny)
                {
                    denied.Add(rule);
                }
            }
            if (allowed.Any())
            {
                return formatter.Format(allowed, RuleOutcome.Allow, Operator.Or);
            }
            if (denied.Any())
            {
                return formatter.Format(denied, RuleOutcome.Deny, Operator.Or);
            }
            if (unavailable.Any())
            {
                return formatter.Format(unavailable, RuleOutcome.Unavailable, Operator.Or);
            }
            return new Rule(RuleOutcome.Ignored, string.Empty);
        }
    }
}
