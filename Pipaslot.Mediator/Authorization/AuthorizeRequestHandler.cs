﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

public class AuthorizeRequestHandler(IServiceProvider serviceProvider) : IMediatorHandler<AuthorizeRequest, AuthorizeRequestResponse>
{
    public async Task<AuthorizeRequestResponse> Handle(AuthorizeRequest action, CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetActionHandlers(action.Action);
        var policyResult = await PolicyResolver.GetPolicyRules(serviceProvider, action.Action, handlers, cancellationToken).ConfigureAwait(false);
        var rootNode = policyResult.Reduce();
        var formatter = serviceProvider.GetRequiredService<INodeFormatter>();
        var reason = formatter.Format(rootNode);
        var accessType = rootNode.Outcome.ToAccessType();
        return new AuthorizeRequestResponse
        {
            Access = accessType, Reason = reason, IsIdentityStatic = policyResult.RulesRecursive.All(r => r.Scope == RuleScope.Identity)
        };
    }
}