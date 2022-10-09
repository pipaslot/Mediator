using Pipaslot.Mediator.Abstractions;
using System;
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
            var rules = await PolicyResolver.GetPolicyRules(_serviceProvider, action.Action, handlers, cancellationToken);
            var notGrantedRules = rules.Rules
                .Where(r => !r.Granted);
            return new IsAuthorizedRequestResponse
            {
                IsAuthorized = notGrantedRules.Count() == 0
            };
        }
    }
}
