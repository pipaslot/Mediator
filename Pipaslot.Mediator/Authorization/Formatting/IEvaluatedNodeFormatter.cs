using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization.Formatting
{
    /// <summary>
    /// Service intercepting rule evaluation. Can be used for message/reason customization or for translations
    /// </summary>
    public interface IEvaluatedNodeFormatter
    {
        /// <summary>
        /// Format/Convert single incoming rule
        /// </summary>
        FormatedNode FormatSingle(EvaluatedNode node, RuleOutcome outcome);

        /// <summary>
        /// Format multiple rules having the same <see cref="RuleOutcome"/>. The minimal amount of incoming rules is 2.
        /// </summary>
        /// <param name="nodes">Rules with the same outcome</param>
        /// <param name="outcome">Outcome of the all rules</param>
        /// <param name="operator">Relation between the rules</param>
        /// <returns>Reduced rules to single one</returns>
        FormatedNode FormatMultiple(EvaluatedNode[] nodes, RuleOutcome outcome, Operator @operator);
    }

    public static class RuleFormatterExtensions
    {
        /// <summary>
        /// Format one or more rules with the same outcome
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static EvaluatedNode Format(this IEvaluatedNodeFormatter formatter, List<EvaluatedNode> nodes, RuleOutcome outcome, Operator @operator)
        {
            if (nodes.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nodes), "The collection can not be empty.");
            }

            var casted = nodes
                .Cast<EvaluatedNode>()
                .ToArray();
            var pair = casted.Count() == 1
                ? formatter.FormatSingle(casted.First(), outcome)
                : formatter.FormatMultiple(casted, outcome, @operator);
            return new EvaluatedNode(pair.Kind, outcome, pair.Reason);
        }
    }
}