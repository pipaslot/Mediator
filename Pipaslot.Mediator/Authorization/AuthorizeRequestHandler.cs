using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization.Formatting;
using Pipaslot.Mediator.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Handles <see cref="AuthorizeRequest"/>; registered automatically by <c>AddMediator</c>.
/// </summary>
public class AuthorizeRequestHandler(IServiceProvider services, INodeFormatter formatter, MediatorConfigurator configurator)
    : IMediatorHandler<AuthorizeRequest, AuthorizeRequestResponse>
{
    /// <summary>
    /// Evaluates the same policies as <see cref="AuthorizationMiddleware"/>, but returns the outcome with a formatted reason
    /// instead of throwing. An action without any policy is reported as <see cref="AccessType.Deny"/>.
    /// </summary>
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