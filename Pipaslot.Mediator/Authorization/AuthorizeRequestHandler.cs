using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization.Formatting;
using Pipaslot.Mediator.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

public class AuthorizeRequestHandler(IServiceProvider services, INodeFormatter formatter, MediatorConfigurator configurator)
    : IMediatorHandler<AuthorizeRequest, AuthorizeRequestResponse>
{
    public async Task<AuthorizeRequestResponse> Handle(AuthorizeRequest action, CancellationToken cancellationToken)
    {
        var actionType = action.Action.GetType();
        var handlerExecutor = services.GetHandlerExecutor(configurator.ReflectionCache, actionType);
        var handlers = handlerExecutor.GetHandlers(services);
        var policyResult = await PolicyResolver.GetPolicyRules(services, action.Action, handlers, cancellationToken).ConfigureAwait(false);
        var rootNode = policyResult.Reduce();
        var reason = formatter.Format(rootNode);
        var accessType = rootNode.Outcome.ToAccessType();
        return new AuthorizeRequestResponse
        {
            Access = accessType, Reason = reason, IsIdentityStatic = policyResult.RulesRecursive.All(r => r.Scope == RuleScope.Identity)
        };
    }
}