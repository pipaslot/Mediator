using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization.Formatting
{
    /// <summary>
    /// Service intercepting rule evaluation. Can be used for message/reason customization or for translations
    /// </summary>
    public interface IRuleFormatter
    {
        /// <summary>
        /// Format/Convert single incomming rule
        /// </summary>
        IRule FormatSingle(IRule rule, RuleOutcome outcome);
        /// <summary>
        /// Format multiple rules having the same <see cref="RuleOutcome"/>. The minimal amount of incomming rules is 2.
        /// </summary>
        /// <param name="rules">Rules with the same outcome</param>
        /// <param name="outcome">Outcome of the all rules</param>
        /// <param name="operator">Relation between the rules</param>
        /// <returns>Reduced rules to single one</returns>
        IRule FormatMultiple(IRule[] rules, RuleOutcome outcome, Operator @operator);
    }

    public static class RuleFormatterExtensions
    {
        /// <summary>
        /// Format one or more rules with the same outcome
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IEvaluatedRule Format(this IRuleFormatter formatter, List<IRule> rules, RuleOutcome outcome, Operator @operator)
        {
            if (rules.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rules), "The collection can not be empty.");
            }
            var casted = rules.Cast<IRule>().ToArray();
            var pair = rules.Count == 1
                ? formatter.FormatSingle(casted.First(), outcome)
                : formatter.FormatMultiple(casted, outcome, @operator);
            return new EvaluatedRule(pair, outcome);
        }
    }
}