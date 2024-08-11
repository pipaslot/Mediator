using System.Linq;
using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization.Formatting
{
    public class DefaultEvaluatedNodeFormatter : IEvaluatedNodeFormatter
    {
        public virtual FormatedNode FormatMultiple(EvaluatedNode[] nodes, RuleOutcome outcome, Operator @operator)
        {
            var notEmpty = nodes
                .Where(r => !string.IsNullOrWhiteSpace(r.Value))
                .ToArray();
            if (notEmpty.Length == 0)
            {
                // If the value/reason is empty, then we have nothing to format, so we keep it empty
                return new FormatedNode(EvaluatedNode.AlreadyFormated, string.Empty);
            }

            if (notEmpty.Length == 1)
            {
                return FormatSingle(notEmpty.First(), outcome, false);
            }

            var operation = FormatOperator(@operator);
            var sets = nodes
                .Select(g => FormatSingle(g, outcome, true))
                .Select(g => g.Reason)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToArray();
            var joined = $"{string.Join(operation, sets)}";
            return new FormatedNode(EvaluatedNode.JoinedFormatedRuleName, joined);
        }

        public virtual FormatedNode FormatSingle(EvaluatedNode node, RuleOutcome outcome)
        {
            return FormatSingle(node, outcome, false);
        }

        protected virtual FormatedNode FormatSingle(EvaluatedNode node, RuleOutcome outcome, bool fromMultiple)
        {
            if (node.Kind == IdentityPolicy.AuthenticationPolicyName)
            {
                if (outcome == RuleOutcome.Allow)
                {
                    return new FormatedNode(EvaluatedNode.AlreadyFormated, FormatAnonymousAuthentication(node));
                }

                if (node.Value == IdentityPolicy.AuthenticatedValue)
                {
                    return new FormatedNode(EvaluatedNode.AlreadyFormated, FormatRequiredAuthentication(node));
                }
            }

            if (node.Kind == ClaimTypes.Role)
            {
                // The node represent evaluation of user role from his identity
                return new FormatedNode(EvaluatedNode.AlreadyFormated, FormatRole(node));
            }

            if (node.Kind == EvaluatedNode.AlreadyFormated || node.Kind == Rule.DefaultName)
            {
                // The rule was not transformed, and we only propagate it from the tree leaves to the root
                return node.ToFormatedNode();
            }

            if (node.Kind == EvaluatedNode.JoinedFormatedRuleName)
            {
                return string.IsNullOrWhiteSpace(node.Value) || !fromMultiple
                    ? node.ToFormatedNode()
                    : new FormatedNode(EvaluatedNode.AlreadyFormated, WrapMultipleRules(node));
            }

            if (outcome == RuleOutcome.Allow)
            {
                // Only propagate the allowed node
                return new FormatedNode(EvaluatedNode.AlreadyFormated, FormatAllowedNode(node));
            }

            // Not recognized node. Format it as required Role or Claim
            return new FormatedNode(EvaluatedNode.AlreadyFormated, FormatDefault(node));
        }

        /// <summary>
        /// Format node/rule which was allowed
        /// </summary>
        protected virtual string FormatAllowedNode(EvaluatedNode node)
        {
            return string.Empty;
        }

        /// <summary>
        /// Format default required claim/node with unknown kind
        /// </summary>
        protected virtual string FormatDefault(EvaluatedNode node)
        {
            return $"{node.Kind} '{node.Value}' is required.";
        }

        /// <summary>
        /// Multiple rules were detected and can be formated together to visually and logically clear the output.
        /// Adding bracket in case like: (A AND B) OR C
        /// </summary>
        protected virtual string WrapMultipleRules(EvaluatedNode node)
        {
            return $"({node.Value})";
        }

        /// <summary>
        /// Convert the value "Anonymous" to sentence informing that the authentication is not needed. ( we keep it empty because nobody cares about such a reason)
        /// </summary>
        protected virtual string FormatAnonymousAuthentication(EvaluatedNode node)
        {
            // We do not need to print the reason
            return string.Empty;
        }

        /// <summary>
        /// Convert the value "Authenticated" to sentence informing user about mandatory authentication
        /// </summary>
        protected virtual string FormatRequiredAuthentication(EvaluatedNode node)
        {
            return "User has to be authenticated.";
        }

        /// <summary>
        /// Format mandatory role
        /// </summary>
        protected virtual string FormatRole(EvaluatedNode node)
        {
            return $"Role '{node.Value}' is required.";
        }

        /// <summary>
        /// Format operator used between multiple rules in one RuleSet
        /// </summary>
        protected virtual string FormatOperator(Operator @operator)
        {
            return @operator == Operator.And
                ? " AND "
                : @operator == Operator.Or
                    ? " OR "
                    : " ";
        }
    }
}