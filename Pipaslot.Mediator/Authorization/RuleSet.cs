using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{

    /// <summary>
    /// Provides connection between policies and rules
    /// Define one or more rules aggregated with AND or OR operator.
    /// By wrapping two RuleSets in parent RuleSet you can define condition like: ( ( Rule1 OR Rule2 ) AND ( Rule3 OR Rule4 ) )
    /// </summary>
    public class RuleSet : IPolicy
    {
        public Operator Operator { get; }
        public List<Rule> Rules { get; set; } = new List<Rule>();
        public List<RuleSet> RuleSets { get; set; } = new List<RuleSet>();

        /// <summary>
        /// Iterate through all rules and rule sets
        /// </summary>
        public IEnumerable<Rule> RulesRecursive => Rules.Concat(RuleSets.SelectMany(s => s.RulesRecursive));

        public RuleSet(Operator @operator = Operator.Add)
        {
            Operator = @operator;
        }

        public RuleSet(params RuleSet[] sets) : this(Operator.Add, sets)
        {
        }

        public RuleSet(Operator @operator, ICollection<RuleSet> sets)
        {
            Operator = @operator;
            RuleSets.AddRange(sets);
        }
        public RuleSet(params Rule[] rules) : this(Operator.Add, rules)
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

        public IEvaluatedRule Evaluate(IRuleFormatter formatter)
        {
            var evaluatedRules = RuleSets
                .Select(s => s.Evaluate(formatter))
                .ToArray();
            
            var rules = evaluatedRules
                .Concat(Rules);
            if (Operator == Operator.Add)
            {
                return ReduceWithAdd(rules, formatter);
            }
            if (Operator == Operator.And)
            {
                return ReduceWithAnd(rules, formatter);
            }
            if (Operator == Operator.Or)
            {
                return ReduceWithOr(rules, formatter);
            }
            throw new NotSupportedException($"Operator '{Operator}' can not be used for RuleSet.");
        }

        private IEvaluatedRule ReduceWithAdd(IEnumerable<IEvaluatedRule> rules, IRuleFormatter formatter)
        {
            var finalOperator = Operator.Add;
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
                return formatter.Format(unavailable, RuleOutcome.Unavailable, finalOperator);
            }
            if (denied.Any())
            {
                return formatter.Format(denied, RuleOutcome.Deny, finalOperator);
            }
            if (allowed.Any())
            {
                return formatter.Format(allowed, RuleOutcome.Allow, finalOperator);
            }
            return new Rule(RuleOutcome.Ignored, string.Empty);
        }
        private IEvaluatedRule ReduceWithAnd(IEnumerable<IEvaluatedRule> rules, IRuleFormatter formatter)
        {
            var finalOperator = Operator.And;
            var denied = new List<IRule>();
            var unavailable = new List<IRule>();
            var allowed = new List<IRule>();
            foreach (var rule in rules)
            {
                var outcome = rule.Outcome;
                if (outcome == RuleOutcome.Unavailable)
                {
                    unavailable.Add(rule);
                }
                if (outcome == RuleOutcome.Deny || outcome == RuleOutcome.Ignored)
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
                return formatter.Format(unavailable, RuleOutcome.Unavailable, finalOperator);
            }
            if (denied.Any())
            {
                return formatter.Format(denied, RuleOutcome.Deny, finalOperator);
            }
            if (allowed.Any())
            {
                return formatter.Format(allowed, RuleOutcome.Allow, finalOperator);
            }
            return new Rule(RuleOutcome.Deny, string.Empty);
        }

        private IEvaluatedRule ReduceWithOr(IEnumerable<IEvaluatedRule> rules, IRuleFormatter formatter)
        {
            var finalOperator = Operator.Or;
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
                return formatter.Format(allowed, RuleOutcome.Allow, finalOperator);
            }
            if (denied.Any())
            {
                return formatter.Format(denied, RuleOutcome.Deny, finalOperator);
            }
            if (unavailable.Any())
            {
                return formatter.Format(unavailable, RuleOutcome.Unavailable, finalOperator);
            }
            return new Rule(RuleOutcome.Ignored, string.Empty);
        }

        public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            return Task.FromResult(this);
        }

#if !NETSTANDARD
        public static RuleSet operator +(RuleSet set, Rule rule)
        {
            var res = new RuleSet(Operator.Add);
            res.RuleSets.Add(set);
            res.Rules.Add(rule);
            return res;
        }
        public static RuleSet operator +(Rule rule, RuleSet set)
        {
            var res = new RuleSet(Operator.Add);
            res.RuleSets.Add(set);
            res.Rules.Add(rule);
            return res;
        }

        public static RuleSet operator +(RuleSet set1, RuleSet set2)
        {
            var res = new RuleSet(Operator.Add);
            res.RuleSets.Add(set1);
            res.RuleSets.Add(set2);
            return res;
        }

        public static RuleSet operator |(RuleSet set, Rule rule)
        {
            var res = new RuleSet(Operator.Or);
            res.RuleSets.Add(set);
            res.Rules.Add(rule);
            return res;
        }

        public static RuleSet operator |(Rule rule, RuleSet set)
        {
            var res = new RuleSet(Operator.Or);
            res.RuleSets.Add(set);
            res.Rules.Add(rule);
            return res;
        }
        public static RuleSet operator |(RuleSet set1, RuleSet set2)
        {
            var res = new RuleSet(Operator.Or);
            res.RuleSets.Add(set1);
            res.RuleSets.Add(set2);
            return res;
        }
#endif
    }
}
