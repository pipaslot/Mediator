using System.Linq;
using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// Provide default semantics for reason message formating
/// </summary>
public class DefaultNodeFormatter : INodeFormatter
{
    public string Format(IRecursiveNode node)
    {
        var formated = ConvertRecursive(node);
        return FormatMessagePrefix(node.Outcome) + formated.Reason.Trim();
    }

    protected virtual string FormatMessagePrefix(RuleOutcome outcome)
    {
        return outcome == RuleOutcome.Allow
            ? ""
            : "Policy rules not matched for the current user: ";
    }

    /// <summary>
    /// Recursion converting the processing from "from root to leaves" to "from leaves to root"
    /// Recursion starting from children to the root and formating the reason
    /// </summary>
    protected virtual FormatedNode ConvertRecursive(IRecursiveNode current, IRecursiveNode? parent = null)
    {
        var children = current.Children
            .Select(n => ConvertRecursive(n, current))
            .Where(n => !string.IsNullOrWhiteSpace(n.Reason))
            .ToArray();

        if (children.Length == 0)
        {
            // The current node is the final one
            return ConvertNode(current);
        }

        if (children.Length == 1)
        {
            return children.First();
        }

        var nonEmptyChildrenCount = parent?.Children
            .Select(n => ConvertRecursive(n, current))
            .Count(n => !string.IsNullOrWhiteSpace(n.Reason)) ?? 1;
        var currentNodeWillBeCombined =  nonEmptyChildrenCount > 1;
        return ConvertJoin(current.Operator, children, currentNodeWillBeCombined);
    }

    protected virtual FormatedNode ConvertJoin(Operator @operator, FormatedNode[] nodes, bool wrapStatement)
    {
        var operation = FormatOperator(@operator);
        var formatedNodes = nodes
            .Select(n => n.Reason);
        var joined = string.Join(operation, formatedNodes);
        if (wrapStatement)
        {
            return new FormatedNode(FormatJoin(joined));
        }

        return new FormatedNode(joined);
    }

    protected virtual FormatedNode ConvertNode(INode node)
    {
        if (node is FormatedNode formatedNode)
        {
            // The rule was not transformed, and we only propagate it from the tree leaves to the root
            return formatedNode;
        }

        if (node is RuleNode ruleNode)
        {
            return ConvertRuleNode(ruleNode);
        }

        if (node is RuleSetNode)
        {
            return new FormatedNode(string.Empty);
        }

        // Not recognized node. Format it as required Role or Claim
        return new FormatedNode(FormatUnknown(node));
    }

    protected virtual FormatedNode ConvertRuleNode(RuleNode ruleNode)
    {
        if (ruleNode.Rule.Name == Rule.DefaultName)
        {
            // Raw rule containing user-friendly message already
            return new FormatedNode(ruleNode.Rule.Value);
        }

        if (ruleNode.Rule.Name == IdentityPolicy.AuthenticationPolicyName)
        {
            return new FormatedNode(FormatAuthenticationRule(ruleNode));
        }

        if (ruleNode.Rule.Name == ClaimTypes.Role)
        {
            // The node represent evaluation of user role from his identity
            return new FormatedNode(FormatRoleRule(ruleNode));
        }
        return new FormatedNode(FormatDefaultRule(ruleNode));
    }

    /// <summary>
    /// Wrap multiple reasons when combining with another set of reasons. For example wraps "A AND B" when applied with C in statement like "(A AND B) OR C"
    /// </summary>
    /// <param name="reasons">Combination of two or more reasons.</param>
    /// <returns></returns>
    protected virtual string FormatJoin(string reasons)
    {
        return $"({reasons})";
    }

    /// <summary>
    /// Format mandatory role
    /// </summary>
    protected virtual string FormatRoleRule(RuleNode node)
    {
        if ( node.Outcome == RuleOutcome.Allow)
        {
            return string.Empty;
        }
        return $"Role '{node.Rule.Value}' is required.";
    }

    protected virtual string FormatAuthenticationRule(RuleNode ruleNode)
    {
        if (ruleNode.Outcome == RuleOutcome.Allow)
        {
            return FormatAnonymousAuthentication(ruleNode);
        }

        if (ruleNode.Rule.Value == IdentityPolicy.AuthenticatedValue)
        {
            return FormatRequiredAuthentication(ruleNode);
        }

        return FormatDefaultRule(ruleNode);
    }
    

    protected virtual string FormatDefaultRule(RuleNode node)
    {
        if (node.Outcome == RuleOutcome.Allow)
        {
            return string.Empty;
        }
        return $"{node.Rule.Name} '{node.Rule.Value}' is required.";
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
    protected virtual string FormatUnknown(INode node)
    {
        return "Unknown node type " + node.GetType().Name;
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