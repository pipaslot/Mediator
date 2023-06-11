using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    internal static class PolicyResolver
    {
        internal static async Task CheckPolicies(IServiceProvider services, IMediatorAction action, object[] handlers, CancellationToken cancellationToken)
        {
            var ruleSet = await GetPolicyRules(services, action, handlers, cancellationToken).ConfigureAwait(false);

            var formatter = services.GetRequiredService<IRuleFormatter>();
            var aggregatedRule = ruleSet
                .Evaluate(formatter);
            var access = aggregatedRule
                .Outcome
                .ToAccessType();
            if (ruleSet.RuleSets.Count == 0 && ruleSet.Rules.Count == 0)
            {
                throw AuthorizationException.NoAuthorization(action.GetActionName());
            }
            if (access != AccessType.Allow)
            {
                throw AuthorizationRuleNotMetException.Create(ruleSet, aggregatedRule.Value);
            }
        }

        public static async Task<RuleSet> GetPolicyRules(IServiceProvider services, IMediatorAction action, object[] handlers, CancellationToken cancellationToken)
        {
            var policies = await GetPolicies(action, handlers, cancellationToken).ConfigureAwait(false);
            return await ConvertPoliciesToRules(policies, services, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<RuleSet> ConvertPoliciesToRules(ICollection<IPolicy> policies, IServiceProvider services, CancellationToken cancellationToken)
        {
            var rules = new RuleSet(Operator.And);
            foreach (var policy in policies)
            {
                var resolvedRules = await policy.Resolve(services, cancellationToken).ConfigureAwait(false);
                rules.RuleSets.Add(resolvedRules);
            }
            return rules;
        }

        internal static async Task<List<IPolicy>> GetPolicies(IMediatorAction action, object[] handlers, CancellationToken cancellationToken)
        {
            var result = new List<IPolicy>();
            var actionPolicies = GetActionPolicies(action);
            result.AddRange(actionPolicies);

            var handlerPolicies = await GetHandlerPolicies(action, handlers, cancellationToken).ConfigureAwait(false);
            result.AddRange(handlerPolicies);

            return result;
        }

        internal static bool HasActionPolicies(Type action, object[] handlers)
        {
            if (action.IsAssignableFrom(typeof(IActionAuthorization))
                || GetPolicyAttributes(action).Any())
            {
                return true;
            }
            foreach (var handler in handlers)
            {
                if (handler is IHandlerAuthorizationMarker
                  || GetPolicyAttributes(handler.GetType()).Any())
                {
                    return true;
                }
            }
            return false;
        }


        private static IEnumerable<IPolicy> GetActionPolicies(IMediatorAction action)
        {
            if (action is IActionAuthorization aa)
            {
                yield return aa.Authorize();
            }
            var attributes = GetPolicyAttributes(action);
            foreach (var attribute in attributes)
            {
                yield return attribute;
            }
        }

        private static async Task<ICollection<IPolicy>> GetHandlerPolicies(IMediatorAction action, object[] handlers, CancellationToken cancellationToken)
        {
            var result = new List<IPolicy>();
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
                        var policy = methodResult as IPolicy 
                            ?? throw MediatorException.NullInsteadOfPolicy(handlerType?.FullName ?? string.Empty);
                        result.Add(policy);
                        isAuthorized = true;
                    }
                    if (interfaces.Any(i => i.GetGenericTypeDefinition() == asyncType))
                    {
                        var method = handlerType.GetMethod(nameof(IHandlerAuthorizationAsync<IMediatorAction>.AuthorizeAsync));
                        var task = (Task?)method!.Invoke(handler, new object[] { action, cancellationToken })!
                            ?? throw MediatorException.NullInsteadOfPolicy(handlerType?.FullName ?? string.Empty);
                        await task.ConfigureAwait(false);
                        var resultProperty = task.GetType().GetProperty("Result");
                        var taskResult = resultProperty?.GetValue(task) as IPolicy
                            ?? throw MediatorException.NullInsteadOfPolicy(handlerType?.FullName ?? string.Empty);
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

        private static IPolicy[] GetPolicyAttributes(IMediatorAction action) => GetPolicyAttributes(action
                .GetType());

        private static IPolicy[] GetPolicyAttributes(Type type) => type
                .GetCustomAttributes(true)
                .Union(type
                    .GetInterfaces()
                    .SelectMany(i => i.GetCustomAttributes(true)))
                .Where(a => a is IPolicy)
                .Cast<IPolicy>()
                .ToArray();

    }
}
