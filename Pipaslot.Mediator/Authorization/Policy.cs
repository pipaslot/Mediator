using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    /// <summary>
    /// Helper class to combine multiple policies via logical operators
    /// </summary>
    public sealed class Policy : List<IPolicy>, IPolicy
    {
        public Operator Operator { get; }

        public Policy(Operator @operator)
        {
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
            var res = new RuleSet(Operator);
            foreach (var policy in this)
            {
                var ruleSet = await policy.Resolve(services, cancellationToken);
                res.RuleSets.Add(ruleSet);
            }
            return res;
        }
    }
}
