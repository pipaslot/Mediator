using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
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
            var notGrantedRules = policyResult.RulesRecursive
                .Where(r => !r.Granted);
            var ruleSets = policyResult.RuleSetsRecursive;
            var formatter = _serviceProvider.GetService<IRuleSetFormatter>() ?? RuleSetFormatter.Instance;
            var reason = formatter.FormatReason(policyResult);
            return new IsAuthorizedRequestResponse
            {
                IsAuthorized = policyResult.IsGranted(),
                Reason = reason,
                RuleSets = MapRuleSet(policyResult.RuleSets),
                IsIdentityStatic = ruleSets.All(r => r.Reproducibility == RuleSetReproducibility.IdentityStatic)
            };
        }

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
