using Pipaslot.Mediator.Middlewares;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    public static class MediatorContextExtensions
    {
        internal static async Task CheckPolicies(this MediatorContext context)
        {
            var rules = await context.GetPolicyRules();
            if (!rules.Any())
            {
                throw AuthorizationException.NoAuthorization(context.ActionIdentifier);
            }
            if (rules.Any(r => !r.Granted))
            {
                throw AuthorizationException.RuleNotMet(rules);
            }
        }

        public static Task<ICollection<Rule>> GetPolicyRules(this MediatorContext context)
        {
            return PolicyResolver.GetPolicyRules(context.Services, context.Action, context.GetHandlers(), context.CancellationToken);
        }
    }
}
