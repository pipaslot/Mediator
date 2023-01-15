﻿using Pipaslot.Mediator.Authorization.RuleSetFormatters;
using System;
using System.Collections.Generic;
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

        public RuleOutcome GetRuleOutcome()
        {
            var outcomes = Rules
                .Select(r => r.Outcome)
                .Concat(RuleSets.Select(s => s.GetRuleOutcome()));
            if (Operator == Operator.And)
            {
                return ReduceWithAnd(outcomes);
            }
            if (Operator == Operator.Or)
            {
                return ReduceWithOr(outcomes);
            }
            throw new NotImplementedException($"Unknown operator '{Operator}'");
        }

        private RuleOutcome ReduceWithAnd(IEnumerable<RuleOutcome> outcomes)
        {
            var result = RuleOutcome.Ignored;
            foreach (var outcome in outcomes)
            {
                if (outcome == RuleOutcome.Ignored)
                {
                    continue;
                }
                if (outcome == RuleOutcome.Unavailable)
                {
                    return RuleOutcome.Unavailable;
                }
                if(outcome == RuleOutcome.Deny)
                {
                    result = outcome; 
                }
                if (outcome == RuleOutcome.Allow && result != RuleOutcome.Deny)
                {
                    result = outcome;
                }
            }
            return result;
        }

        private RuleOutcome ReduceWithOr(IEnumerable<RuleOutcome> outcomes)
        {
            var result = RuleOutcome.Ignored;
            foreach (var outcome in outcomes)
            {
                if (outcome == RuleOutcome.Ignored)
                {
                    continue;
                }
                if (outcome == RuleOutcome.Unavailable && outcome != RuleOutcome.Deny)
                {
                    result = outcome;
                }
                if (outcome == RuleOutcome.Deny)
                {
                    result = outcome;
                }
                if (outcome == RuleOutcome.Allow)
                {
                    return outcome;
                }
            }
            return result;
        }
    }
}
