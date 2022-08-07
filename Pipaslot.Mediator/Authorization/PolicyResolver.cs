using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    internal static class PolicyResolver
    {
        public static async Task<ICollection<Rule>> GetPolicyRules(IServiceProvider services, IMediatorAction action, object[] handlers, CancellationToken cancellationToken)
        {
            var policies = await GetPolicies(action, handlers, cancellationToken);
            var rules = new List<Rule>();
            foreach (var policy in policies)
            {
                var resolvedRules = await policy.Resolve(services, cancellationToken);
                rules.AddRange(resolvedRules);
            }
            return rules;
        }

        public static async Task<List<IPolicy>> GetPolicies(IMediatorAction action, object[] handlers, CancellationToken cancellationToken)
        {
            var result = new List<IPolicy>();
            if (action is IActionAuthorization aa)
            {
                result.Add(aa.Authorize());
            }
            var attributes = GetPolicyAttributes(action.GetType());
            foreach (var attribute in attributes)
            {
                result.Add(attribute);
            }
            var syncType = typeof(IHandlerAuthorization<>);
            var asyncType = typeof(IHandlerAuthorizationAsync<>);
            var authorizedHandlers = new HashSet<object>();
            var unauthorizedHandlers = new HashSet<object>();
            foreach (var handler in handlers)
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
                        var methodResult = method!.Invoke(handler, new object[] { action })!;
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
                        var task = (Task?)method!.Invoke(handler, new object[] { action, cancellationToken })!;
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
