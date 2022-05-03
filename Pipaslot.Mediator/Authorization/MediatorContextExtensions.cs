using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
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

        public static async Task<ICollection<Rule>> GetPolicyRules(this MediatorContext context)
        {
            var policies = await context.GetPolicies();
            var rules = new List<Rule>();
            foreach (var policy in policies)
            {
                var resolvedRules = await policy.Resolve(context.Services, context.CancellationToken);
                rules.AddRange(resolvedRules);
            }
            if (rules.Any(r => !r.Granted))
            {
                throw AuthorizationException.RuleNotMet(rules);
            }
            return rules;
        }

        internal static async Task<List<IPolicy>> GetPolicies(this MediatorContext context)
        {
            var result = new List<IPolicy>();
            if (context.Action is IActionAuthorization aa)
            {
                result.Add(aa.Authorize());
            }
            var attributes = GetPolicyAttributes(context.Action.GetType());
            foreach (var attribute in attributes)
            {
                result.Add(attribute);
            }
            var syncType = typeof(IHandlerAuthorization<>);
            var asyncType = typeof(IHandlerAuthorizationAsync<>);
            var authorizedHandlers = new HashSet<object>();
            var unauthorizedHandlers = new HashSet<object>();
            foreach (var handler in context.GetHandlers())
            {
                var isAuthorized = false;
                var handlerType = handler.GetType();
                var handlerAttributes = GetPolicyAttributes(handlerType);
                foreach (var attribute in handlerAttributes)
                {
                    result.Add(attribute);
                    isAuthorized = true;
                }
                if (handler is IHandlerAuthorizationMarker)
                {
                    var interfaces = handlerType
                        .GetInterfaces()
                        .Where(i => i.IsGenericType);
                    if (interfaces.Any(i => i.GetGenericTypeDefinition() == syncType))
                    {
                        var method = handlerType.GetMethod(nameof(IHandlerAuthorization<IMediatorAction>.Authorize));
                        var methodResult = method!.Invoke(handler, new object[] { context.Action })!;
                        var policy = methodResult as IPolicy;
                        if (policy == null)
                        {
                            throw MediatorException.NullInsteadOfPolicy(handlerType.FullName);
                        }
                        result.Add(policy);
                        isAuthorized = true;
                    }
                    if (interfaces.Any(i => i.GetGenericTypeDefinition() == asyncType))
                    {
                        var method = handlerType.GetMethod(nameof(IHandlerAuthorizationAsync<IMediatorAction>.AuthorizeAsync));
                        var task = (Task?)method!.Invoke(handler, new object[] { context.Action, context.CancellationToken })!;
                        if (task == null)
                        {
                            throw MediatorException.NullInsteadOfPolicy(handlerType.FullName);
                        }
                        await task.ConfigureAwait(false);
                        var resultProperty = task.GetType().GetProperty("Result");
                        var taskResult = resultProperty?.GetValue(task) as IPolicy;
                        if (taskResult == null)
                        {
                            throw MediatorException.NullInsteadOfPolicy(handlerType.FullName);
                        }
                        result.Add(taskResult);
                        isAuthorized = true;
                    }
                }
                if (isAuthorized)
                {
                    authorizedHandlers.Add(handler);
                }
                else
                {
                    unauthorizedHandlers.Add(handler);
                }
            }
            if (authorizedHandlers.Any() && unauthorizedHandlers.Any())
            {
                throw AuthorizationException.UnauthorizedHandler(unauthorizedHandlers);
            }
            return result;
        }

        private static IPolicy[] GetPolicyAttributes(Type type) => type
                .GetCustomAttributes(true)
                .Where(a => a is IPolicy)
                .Cast<IPolicy>()
                .ToArray();
    }
}
