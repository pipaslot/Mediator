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
        public PolicyOperator Operator { get; }

        public Policy(PolicyOperator @operator)
        {
            Operator = @operator;
        }
        
        public Policy(PolicyOperator @operator, params IPolicy[] policies) : base(policies)
        {
            Operator = @operator;
        }

        /// <summary>
        /// Combine multiple policies together with AND operator
        /// </summary>
        public static Policy And(params IPolicy[] policies)
        {
            var expression = new Policy(PolicyOperator.And);
            expression.AddRange(policies);
            return expression;
        }

        /// <summary>
        /// Combine multiple policies together with OR operator
        /// </summary>
        public static Policy Or(params IPolicy[] policies)
        {
            var expression = new Policy(PolicyOperator.Or);
            expression.AddRange(policies);
            return expression;
        }

        public async Task<IRuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            var res = new RuleSetCollection(Operator);
            foreach(var policy in this)
            {
                var ruleSet = await policy.Resolve(services, cancellationToken);
                res.Add(ruleSet);
            }
            return res;
        }
    }
}
