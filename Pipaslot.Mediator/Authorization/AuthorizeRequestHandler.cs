using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization.Formatters;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizeRequestHandler : IMediatorHandler<AuthorizeRequest, AuthorizeRequestResponse>
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthorizeRequestHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<AuthorizeRequestResponse> Handle(AuthorizeRequest action, CancellationToken cancellationToken)
        {
            var handlers = _serviceProvider.GetActionHandlers(action.Action);
            var policyResult = await PolicyResolver.GetPolicyRules(_serviceProvider, action.Action, handlers, cancellationToken);
            var formatter = _serviceProvider.GetRequiredService<IRuleSetFormatter>();
            var combinedRule = policyResult.Evaluate(formatter);
            var accessType = combinedRule.Outcome.ToAccessType();
            var isAuthorized = accessType == AccessType.Allow;
            return new AuthorizeRequestResponse
            {
                Access = accessType,
                Reason = combinedRule.Value,
                IsIdentityStatic = policyResult.RulesRecursive.All(r => r.Scope == RuleScope.Identity)
            };
        }
    }
}
