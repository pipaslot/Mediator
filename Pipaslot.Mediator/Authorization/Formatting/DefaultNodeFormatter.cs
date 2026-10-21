using System.Linq;
using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// Provide default semantics for reason message formating
/// </summary>
/// <remarks>
/// The <see cref="INodeFormatter"/> registered by <c>AddMediator</c>. It walks the reduced tree from the leaves up,
/// renders every <see cref="RuleNode"/> to a sentence, drops empty sentences, joins siblings with
/// <see cref="FormatOperator"/> and prepends <see cref="NegativeOutcomeMessagePrefix"/> or
/// <see cref="PositiveOutcomeMessagePrefix"/>.
/// <para>
/// To change wording, prefixes or bracketing, derive from this class, override the relevant <c>protected virtual</c>
/// member (typically <see cref="ConvertRuleNode"/> or one of the <c>Format*Rule</c> methods) and register the subclass
/// with <c>ReplaceNodeFormatter</c>. Implement <see cref="INodeFormatter"/> directly only when the output is not a
/// sentence built from the tree, e.g. a structured or localized message.
/// </para>
/// </remarks>
public class DefaultNodeFormatter : INodeFormatter
{
    /// <summary>
    /// Prepended to the reason for every outcome other than <see cref="RuleOutcome.Allow"/>, including
    /// <see cref="RuleOutcome.Unavailable"/> and <see cref="RuleOutcome.Ignored"/>.
    /// </summary>
    public virtual string NegativeOutcomeMessagePrefix { get; set; } = "Policy rules not matched for the current user: ";
    /// <summary>
    /// Prepended to the reason when the outcome is <see cref="RuleOutcome.Allow"/>. Empty by default. Only visible through
    /// <see cref="AuthorizeRequest"/>, because the authorization middleware formats a reason only when access is not allowed.
    /// </summary>
    public virtual string PositiveOutcomeMessagePrefix { get; set; } = "";
    /// <summary>
    /// When a combination of several reasons is put in parentheses; applied by <see cref="FormatJoin"/>.
    /// </summary>
    public virtual MultipleRuleWrapType MultipleRuleWrapType { get; set; } = MultipleRuleWrapType.Always;

    /// <summary>
    /// Renders <paramref name="node"/> and its descendants to one message: the prefix chosen by
    /// <see cref="FormatMessagePrefix"/> followed by the trimmed reason.
    /// </summary>
    public virtual string Format(IRecursiveNode node)
    {
        var formated = ConvertRecursive(node);
        return FormatMessagePrefix(node.Outcome) + formated.Reason.Trim();
    }

    /// <summary>
    /// Chooses the message prefix for the root outcome: <see cref="PositiveOutcomeMessagePrefix"/> for
    /// <see cref="RuleOutcome.Allow"/>, otherwise <see cref="NegativeOutcomeMessagePrefix"/>.
    /// Override to vary the prefix per outcome, e.g. a different text for <see cref="RuleOutcome.Unavailable"/>.
    /// </summary>
    protected virtual string FormatMessagePrefix(RuleOutcome outcome)
    {
        return outcome == RuleOutcome.Allow
            ? PositiveOutcomeMessagePrefix
            : NegativeOutcomeMessagePrefix;
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
            // pull the node closer to the root of the tree without formating, because it was already formated.
            return children.First();
        }

        var nonEmptyChildrenCount = parent?.Children
            .Select(n => ConvertRecursive(n, current))
            .Count(n => !string.IsNullOrWhiteSpace(n.Reason)) ?? 1;
        var currentNodeWillBeCombined = nonEmptyChildrenCount > 1;
        return ConvertJoin(current.Operator, children, currentNodeWillBeCombined, parent?.Operator);
    }

    /// <summary>
    /// Joins the already formatted reasons of two or more sibling nodes with <see cref="FormatOperator"/>.
    /// </summary>
    /// <param name="operator">Operator of the node whose children are joined.</param>
    /// <param name="nodes">Formatted children; children with an empty reason are already removed.</param>
    /// <param name="wrapStatement">The joined text will itself be combined with a sibling, so it is passed to
    /// <see cref="FormatJoin"/> for optional parentheses. False for a combination that stands alone, e.g. at the root.</param>
    /// <param name="parentOperator">Operator of the parent node, <c>null</c> at the root.</param>
    protected virtual FormatedNode ConvertJoin(Operator @operator, FormatedNode[] nodes, bool wrapStatement, Operator? parentOperator)
    {
        var operation = FormatOperator(@operator);
        var formatedNodes = nodes
            .Select(n => n.Reason);
        var joined = string.Join(operation, formatedNodes);
        var differentOperators = @operator != parentOperator;
        if (wrapStatement)
        {
            return new FormatedNode(FormatJoin(joined, differentOperators));
        }

        return new FormatedNode(joined);
    }

    /// <summary>
    /// Convert any node. Initial method starting the formatting.
    /// </summary>
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

    /// <summary>
    /// The node was detected as Rule. Convert to the node to Formatted node via Format methods depending on the Rule name
    /// </summary>
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
    /// <param name="differentOperators">The reasons are coming from ruleset having different operator. Example "(Rule1 AND Rule2) OR (Rule3 AND Rule4)"</param>
    protected virtual string FormatJoin(string reasons, bool differentOperators)
    {
        if (MultipleRuleWrapType == MultipleRuleWrapType.Always)
        {
            return WrapMultipleRules(reasons);
        }

        if (MultipleRuleWrapType == MultipleRuleWrapType.DifferentOperator && differentOperators)
        {
            return WrapMultipleRules(reasons);
        }

        // Never wrap case
        return reasons;
    }

    /// <summary>
    /// Format multiple rules, when starting applying with another rule set. See: <see cref="MultipleRuleWrapType"/> to control the flow.
    /// </summary>
    protected virtual string WrapMultipleRules(string reasons)
    {
        return $"({reasons})";
    }

    /// <summary>
    /// Format mandatory role
    /// </summary>
    protected virtual string FormatRoleRule(RuleNode node)
    {
        if (node.Outcome == RuleOutcome.Allow)
        {
            return string.Empty;
        }

        return $"Role '{node.Rule.Value}' is required.";
    }

    /// <summary>
    /// Format rule detecting if the user is anonymous or authenticated and whether the authentication is mandatory.
    /// </summary>
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

    /// <summary>
    /// The rule was not recognized as any standard rule.
    /// </summary>
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
        return @operator switch
        {
            Operator.And => " AND ",
            Operator.Or => " OR ",
            _ => " "
        };
    }
}