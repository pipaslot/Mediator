using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization.RuleSetFormatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    public class IsAuthorizedRequestHandler : IMediatorHandler<IsAuthorizedRequest, IsAuthorizedRequestResponse>
    {
        private readonly IServiceProvider _serviceProvider;

        public IsAuthorizedRequestHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IsAuthorizedRequestResponse> Handle(IsAuthorizedRequest action, CancellationToken cancellationToken)
        {
            var handlers = _serviceProvider.GetActionHandlers(action.Action);
            var policyResult = await PolicyResolver.GetPolicyRules(_serviceProvider, action.Action, handlers, cancellationToken);
            var outcome = policyResult.GetRuleOutcome();
            var accessType = outcome.ToAccessType();
            var isAuthorized = accessType == AccessType.Allow;
            IRuleSetFormatter formatter = isAuthorized
                ? _serviceProvider.GetRequiredService<IAllowedRuleSetFormatter>()
                : _serviceProvider.GetRequiredService<IDeniedRuleSetFormatter>();
            var reason = formatter.Format(policyResult);
            return new IsAuthorizedRequestResponse
            {
                Access = accessType,
                IsAuthorized = isAuthorized,
                Reason = reason,
                RuleSets = MapRuleSet(policyResult.RuleSets),
                IsIdentityStatic = policyResult.RulesRecursive.All(r => r.Scope == RuleScope.Identity)
            };
        }

        [Obsolete]
        private IsAuthorizedRequestResponse.RuleSetDto[] MapRuleSet(List<RuleSet> ruleSets)
        {
            return ruleSets
                .Select(s => new IsAuthorizedRequestResponse.RuleSetDto
                {
                    Operator = s.Operator.ToString().ToUpper(),
                    SubSets = MapRuleSet(s.RuleSets),
                    Rules = s.Rules
                        .Select(r => new IsAuthorizedRequestResponse.RuleDto
                        {
                            Granted = r.Granted,
                            Name = r.Name,
                            Value = r.Value
                        })
                        .ToArray()
                })
                .ToArray();
        }
    }
}
