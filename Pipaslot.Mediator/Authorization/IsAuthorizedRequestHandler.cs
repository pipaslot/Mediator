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
            return new IsAuthorizedRequestResponse
            {
                IsAuthorized = policyResult.IsGranted(),
                RuleSets = /*ReduceDepth(*/MapRuleSet(policyResult.RuleSets)/*)*/,
                IsIdentityStatic = ruleSets.All(r => r.Reproducibility == RuleSetReproducibility.IdentityStatic)
            };
        }

        //private IsAuthorizedRequestResponse.RuleSetDto[] ReduceDepth(IsAuthorizedRequestResponse.RuleSetDto[] ruleSetDtos)
        //{
        //    foreach (var ruleSetDto in ruleSetDtos)
        //    {
        //        var rules = new List<IsAuthorizedRequestResponse.RuleDto>(ruleSetDto.Rules);
        //        var reducedSets = new List<IsAuthorizedRequestResponse.RuleSetDto>();
        //        foreach (var childSet in ReduceDepth(ruleSetDto.SubSets)){
        //            if (childSet.Operator == ruleSetDto.Operator)
        //            {
        //                rules.AddRange(childSet.Rules);
        //            }
        //            else
        //            {
        //                reducedSets.Add(childSet);
        //            }
        //        }
        //        ruleSetDto.SubSets = reducedSets.ToArray();
        //        ruleSetDto.Rules = rules.ToArray();
        //    }
        //    return ruleSetDtos;
        //}

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
