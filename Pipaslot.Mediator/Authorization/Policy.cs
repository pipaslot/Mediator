using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Helper class to combine multiple policies via logical operators
/// </summary>
public sealed class Policy : List<IPolicy>, IPolicy
{
    public Operator Operator { get; }

    public Policy(Operator @operator)
    {
        if (@operator != Operator.And && @operator != Operator.Or)
        {
            throw new NotSupportedException($"Operator '{@operator}' can not be used for Policies.");
        }

        Operator = @operator;
    }

    public Policy(Operator @operator, params IPolicy[] policies) : base(policies)
    {
        Operator = @operator;
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

#if !NETSTANDARD
        public static IPolicy operator &(Policy c1, IPolicy c2)
        {
            return c1.And(c2);
        }

        public static IPolicy operator |(Policy c1, IPolicy c2)
        {
            return c1.Or(c2);
        }
#endif
}