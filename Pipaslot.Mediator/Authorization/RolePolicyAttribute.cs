using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RolePolicyAttribute : Attribute, IPolicy
    {
        private readonly IdentityPolicy _policy;
        public RolePolicyAttribute(params string[] requiredRoles) : this(Operator.And, requiredRoles)
        {
           
        }
        public RolePolicyAttribute(Operator @operator, params string[] requiredRoles)
        {
            if (requiredRoles.Length == 0)
            {
                throw new ArgumentException("Can not be empty collection", nameof(requiredRoles));
            }
            _policy = new IdentityPolicy(@operator);
            foreach (string role in requiredRoles)
            {
                _policy.HasRole(role);
            }
        }

        public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            return _policy.Resolve(services, cancellationToken);
        }
    }
}
