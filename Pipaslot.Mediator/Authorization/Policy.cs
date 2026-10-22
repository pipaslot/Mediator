using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Helper class to combine multiple policies via logical operators
/// </summary>
/// <remarks>
/// The composite of the authorization model: it groups other <see cref="IPolicy"/> values - rules, identity policies or
/// nested policies - under a single AND/OR <see cref="Operator"/>, and resolves them concurrently into one
/// <see cref="RuleSet"/>. Build it with <see cref="And"/>/<see cref="Or"/> or with the <c>&amp;</c>/<c>|</c> operators;
/// nest instances to express mixed conditions such as <c>(a | b) &amp; c</c>.
/// <para>
/// Composition only; the pass/fail outcome lives in the resolved <see cref="RuleSet"/>. A single condition needs no
/// <see cref="Policy"/> at all - return the <see cref="Rule"/> or <see cref="IdentityPolicy"/> directly from
/// <see cref="IHandlerAuthorization{TAction}.Authorize"/>.
/// </para>
/// </remarks>
public sealed class Policy : List<IPolicy>, IPolicy
{
    /// <summary>
    /// How the contained policies are combined. Only <see cref="Operator.And"/> and <see cref="Operator.Or"/> are valid here.
    /// </summary>
    public Operator Operator { get; }

    /// <summary>
    /// Creates an empty policy; add the combined policies with the collection methods.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="operator"/> is <see cref="Operator.Add"/>.</exception>
    public Policy(Operator @operator)
    {
        Operator = ValidateOperator(@operator);
    }

    /// <summary>
    /// Creates the policy already containing <paramref name="policies"/>; <see cref="And"/>/<see cref="Or"/> are the shorter form.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="operator"/> is <see cref="Operator.Add"/>.</exception>
    public Policy(Operator @operator, params IPolicy[] policies) : base(policies)
    {
        Operator = ValidateOperator(@operator);
    }

    private static Operator ValidateOperator(Operator @operator)
    {
        if (@operator != Operator.And && @operator != Operator.Or)
        {
            throw new NotSupportedException($"Operator '{@operator}' can not be used for Policies.");
        }

        return @operator;
    }

    /// <summary>
    /// Combine multiple policies together with AND operator
    /// </summary>
    public static Policy And(params IPolicy[] policies)
    {
        var expression = new Policy(Operator.And);
        expression.AddRange(policies);
        return expression;
    }

    /// <summary>
    /// Combine multiple policies together with OR operator
    /// </summary>
    public static Policy Or(params IPolicy[] policies)
    {
        var expression = new Policy(Operator.Or);
        expression.AddRange(policies);
        return expression;
    }

    /// <summary>
    /// Resolves all contained policies concurrently and nests their rule sets under one <see cref="RuleSet"/> with <see cref="Operator"/>.
    /// </summary>
    public async Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
    {
        var tasks = this
            .Select(policy => policy.Resolve(services, cancellationToken))
            .ToArray();
        await Task.WhenAll(tasks).ConfigureAwait(false);
        var res = new RuleSet(Operator);
        res.RuleSets.AddRange(tasks.Select(t => t.Result));
        return res;
    }

    /// <inheritdoc cref="IPolicy.op_BitwiseAnd"/>
    public static IPolicy operator &(Policy c1, IPolicy c2)
    {
        return c1.And(c2);
    }

    /// <inheritdoc cref="IPolicy.op_BitwiseOr"/>
    public static IPolicy operator |(Policy c1, IPolicy c2)
    {
        return c1.Or(c2);
    }
}