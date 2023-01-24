using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization.Formatters
{
    /// <summary>
    /// Service intercepting rule evaluation. Can be used for message/reason customization or for translations
    /// </summary>
    public interface IRuleSetFormatter
    {
        /// <summary>
        /// Format/Convert single incomming rule
        /// </summary>
        Rule FormatSingle(Rule rule);
        /// <summary>
        /// Format multiple rules having the same <see cref="RuleOutcome"/>. The minimal amount of incomming rules is 2.
        /// </summary>
        /// <param name="rules">Rules with the same outcome</param>
        /// <param name="operator">Relation between the rules</param>
        /// <returns>Reduced rules to single one</returns>
        Rule FormatMultiple(List<Rule> rules, Operator @operator);
    }

    public static class RuleSetFormatterExtensions
    {
        /// <summary>
        /// Format one or more rules
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Rule Format(this IRuleSetFormatter formatter, List<Rule> rules, Operator @operator)
        {
            return rules.Count == 0
                ? throw new ArgumentOutOfRangeException(nameof(rules), "The collection can not be empty.")
                : rules.Count == 1
                ? formatter.FormatSingle(rules.First())
                : formatter.FormatMultiple(rules, @operator);
        }
    }
}