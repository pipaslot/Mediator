using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Provides connection between policies and rules
/// Define one or more rules aggregated with AND or OR operator.
/// By wrapping two RuleSets in parent RuleSet you can define condition like: ( ( Rule1 OR Rule2 ) AND ( Rule3 OR Rule4 ) )
/// </summary>
/// <remarks>
/// What a resolved <see cref="IPolicy"/> becomes - the evaluated tree, not the request to evaluate one. It is kept as a
/// tree rather than collapsed to a boolean so the refusal can be explained: <see cref="Reduce"/> prunes it to the branch
/// that decided the outcome, which the node formatters then render into a human-readable reason for the client.
/// <para>
/// The <see cref="Operator"/> decides how child outcomes combine, and the three of them differ in more than strictness:
/// <see cref="Operator.And"/> treats an <see cref="RuleOutcome.Ignored"/> child as a denial, while
/// <see cref="Operator.Or"/> and <see cref="Operator.Add"/> skip it. A set with no deciding child denies under
/// <see cref="Operator.And"/> and stays <see cref="RuleOutcome.Ignored"/> otherwise - so an empty policy never grants
/// access. Compose with the <c>+</c>/<c>&amp;</c>/<c>|</c> operators rather than filling the collections by hand.
/// </para>
/// </remarks>
public class RuleSet : IPolicy
{
    /// <inheritdoc cref="Rule.Default"/>
    public static readonly RuleSet Default = new (Rule.Default);

    /// <summary>
    /// How the outcomes of <see cref="Rules"/> and <see cref="RuleSets"/> are combined into the outcome of this set.
    /// </summary>
    public Operator Operator { get; }

    /// <summary>
    /// Rules evaluated directly by this set.
    /// </summary>
    public List<Rule> Rules { get; set; } = [];

    /// <summary>
    /// Nested sets, each combining its own children with its own <see cref="Operator"/>.
    /// </summary>
    public List<RuleSet> RuleSets { get; set; } = [];

    /// <summary>
    /// Iterate through all rules and rule sets
    /// </summary>
    public IEnumerable<Rule> RulesRecursive => Rules.Concat(RuleSets.SelectMany(s => s.RulesRecursive));

    public RuleSet() : this(Operator.Add)
    {
    }

    [JsonConstructor]
    public RuleSet(Operator @operator)
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

    /// <summary>
    /// Collapses the tree to the branch responsible for the outcome, dropping everything that did not contribute to it.
    /// This is what the node formatters render into the reason shown to the user.
    /// </summary>
    public IRecursiveNode Reduce()
    {
        var children = RuleSets
            .Select(s => s.Reduce())
            .ToArray();

        var rules = Rules
            .Select(r => (IRecursiveNode)new RuleNode(r));

        var combined = children
            .Concat(rules)
            .ToList();
        if (Operator == Operator.Add)
        {
            return ReduceWithAdd(combined);
        }

        if (Operator == Operator.And)
        {
            return ReduceWithAnd(combined);
        }

        if (Operator == Operator.Or)
        {
            return ReduceWithOr(combined);
        }

        throw new NotSupportedException($"Operator '{Operator}' can not be used for RuleSet.");
    }

    private RuleSetNode ReduceWithAdd(IEnumerable<IRecursiveNode> children)
    {
        var denied = new List<IRecursiveNode>();
        var unavailable = new List<IRecursiveNode>();
        var allowed = new List<IRecursiveNode>();
        foreach (var rule in children)
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
            return new RuleSetNode(this, unavailable, RuleOutcome.Unavailable);
        }

        if (denied.Any())
        {
            return new RuleSetNode(this, denied, RuleOutcome.Deny);
        }

        if (allowed.Any())
        {
            return new RuleSetNode(this, allowed, RuleOutcome.Allow);
        }

        return new RuleSetNode(this, Array.Empty<IRecursiveNode>(), RuleOutcome.Ignored);
    }

    private RuleSetNode ReduceWithAnd(IEnumerable<IRecursiveNode> rules)
    {
        var denied = new List<IRecursiveNode>();
        var unavailable = new List<IRecursiveNode>();
        var allowed = new List<IRecursiveNode>();
        foreach (var rule in rules)
        {
            var outcome = rule.Outcome;
            if (outcome == RuleOutcome.Unavailable)
            {
                unavailable.Add(rule);
            }

            if (outcome is RuleOutcome.Deny or RuleOutcome.Ignored)
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
            return new RuleSetNode(this, unavailable, RuleOutcome.Unavailable);
        }

        if (denied.Any())
        {
            return new RuleSetNode(this, denied, RuleOutcome.Deny);
        }

        if (allowed.Any())
        {
            return new RuleSetNode(this, allowed, RuleOutcome.Allow);
        }

        return new RuleSetNode(this, Array.Empty<IRecursiveNode>(), RuleOutcome.Deny);
    }

    private RuleSetNode ReduceWithOr(IEnumerable<IRecursiveNode> rules)
    {
        var denied = new List<IRecursiveNode>();
        var unavailable = new List<IRecursiveNode>();
        var allowed = new List<IRecursiveNode>();
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
            return new RuleSetNode(this, allowed, RuleOutcome.Allow);
        }

        if (denied.Any())
        {
            return new RuleSetNode(this, denied, RuleOutcome.Deny);
        }

        if (unavailable.Any())
        {
            return new RuleSetNode(this, unavailable, RuleOutcome.Unavailable);
        }

        return new RuleSetNode(this, Array.Empty<IRecursiveNode>(), RuleOutcome.Ignored);
    }

    public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
    {
        return Task.FromResult(this);
    }

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

    public static RuleSet operator &(RuleSet set, Rule rule)
    {
        var res = new RuleSet(Operator.And);
        res.RuleSets.Add(set);
        res.Rules.Add(rule);
        return res;
    }

    public static RuleSet operator &(Rule rule, RuleSet set)
    {
        var res = new RuleSet(Operator.And);
        res.RuleSets.Add(set);
        res.Rules.Add(rule);
        return res;
    }

    public static RuleSet operator &(RuleSet set1, RuleSet set2)
    {
        var res = new RuleSet(Operator.And);
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
}