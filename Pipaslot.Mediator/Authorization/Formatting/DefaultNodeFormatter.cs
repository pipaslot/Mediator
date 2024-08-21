using System.Linq;
using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// Provide default semantics for reason message formating
/// </summary>
public class DefaultNodeFormatter : INodeFormatter
{
    public string NegativeOutcomeMessagePrefix = "Policy rules not matched for the current user: ";
    public string PositiveOutcomeMessagePrefix = "";

    public string Format(IRecursiveNode node)
    {
        var formated = LoopThroughRecursion(node);
        return (node.Outcome == RuleOutcome.Allow
            ? PositiveOutcomeMessagePrefix
            : NegativeOutcomeMessagePrefix) + formated.Reason.Trim();
    }

    /// <summary>
    /// Recursion converting the processing from "from root to leaves" to "from leaves to root"
    /// Recursion starting from children to the root and formating the reason
    /// </summary>
    private INode LoopThroughRecursion(IRecursiveNode current, IRecursiveNode? parent = null)
    {
        var children = current.Children
            .Select(n => LoopThroughRecursion(n, current))
            .Where(n => !string.IsNullOrWhiteSpace(n.Reason))
            .ToArray();

        if (children.Length == 0)
        {
            // The current node is the final one
            return FormatNode(current);
        }

        if (children.Length == 1)
        {
            return FormatNode(children.First());
        }

        var currentNodeWillBeCombined = (parent?.Children.Select(n => LoopThroughRecursion(n, current)).Count(n => !string.IsNullOrWhiteSpace(n.Reason)) ?? 1) > 1;
        return FormatJoin(current.Operator, children, currentNodeWillBeCombined);
    }

    protected virtual FormatedNode FormatJoin(Operator @operator, INode[] nodes, bool wrapStatement)
    {
        var operation = FormatOperator(@operator);
        var formatedNodes = nodes
            .Select(FormatNode)
            .Select(n => n.Reason);
        var joined = string.Join(operation, formatedNodes);
        if (wrapStatement)
        {
            return new FormatedNode(WrapJoinNode(joined));
        }

        return new FormatedNode(joined);
    }

    private INode FormatNode(INode node)
    {
        if (node is FormatedNode)
        {
            // The rule was not transformed, and we only propagate it from the tree leaves to the root
            return node;
        }

        if (node is RuleNode ruleNode)
        {
            if (ruleNode.Rule.Name == Rule.DefaultName)
            {
                // Raw rule containing user-friendly message already
                return new FormatedNode(ruleNode.Rule.Value);
            }

            if (ruleNode.Rule.Name == IdentityPolicy.AuthenticationPolicyName)
            {
                return new FormatedNode(FormatAuthentication(ruleNode));
            }

            if (ruleNode.Rule.Name == ClaimTypes.Role)
            {
                // The node represent evaluation of user role from his identity
                return new FormatedNode(FormatRole(ruleNode));
            }
            return new FormatedNode(FormatRule(ruleNode));
        }

        // Not recognized node. Format it as required Role or Claim
        return new FormatedNode(FormatDefault(node));
    }

    protected virtual string WrapJoinNode(string reason)
    {
        return $"({reason})";
    }

    /// <summary>
    /// Format mandatory role
    /// </summary>
    protected virtual string FormatRole(RuleNode node)
    {
        if ( node.Outcome == RuleOutcome.Allow)
        {
            return string.Empty;
        }
        return $"Role '{node.Rule.Value}' is required.";
    }

    private string FormatRule(RuleNode node)
    {
        if (node.Outcome == RuleOutcome.Allow)
        {
            return string.Empty;
        }
        return $"{node.Rule.Name} '{node.Rule.Value}' is required.";
    }

    private string FormatAuthentication(RuleNode ruleNode)
    {
        if (ruleNode.Outcome == RuleOutcome.Allow)
        {
            return FormatAnonymousAuthentication(ruleNode);
        }

        if (ruleNode.Rule.Value == IdentityPolicy.AuthenticatedValue)
        {
            return FormatRequiredAuthentication(ruleNode);
        }

        return FormatRule(ruleNode);
    }
    
    /// <summary>
    /// Convert the value "Anonymous" to sentence informing that the authentication is not needed. ( we keep it empty because nobody cares about such a reason)
    /// </summary>
    protected virtual string FormatAnonymousAuthentication(RuleNode node)
    {
        // We do not need to print the reason
        return string.Empty;
    }

    /// <summary>
    /// Convert the value "Authenticated" to sentence informing user about mandatory authentication
    /// </summary>
    protected virtual string FormatRequiredAuthentication(RuleNode node)
    {
        return "User has to be authenticated.";
    }

    /// <summary>
    /// Format default required claim/node with unknown kind
    /// </summary>
    protected virtual string FormatDefault(INode node)
    {
        return node.Reason;
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