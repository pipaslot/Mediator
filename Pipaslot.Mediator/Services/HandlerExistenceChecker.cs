using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pipaslot.Mediator.Services;

public class HandlerExistenceChecker : IHandlerExistenceChecker
{
    /// <summary>
    /// We need to ignore handlers on less generic type. For example once command is catch, then we do not expect that generic IHandler will process that command as well.
    /// </summary>
    private readonly HashSet<Type> _alreadyVerified = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly IActionTypeProvider _actionTypeProvider;
    private readonly List<string> _errors = new();

    public HandlerExistenceChecker(IServiceProvider serviceProvider, IActionTypeProvider actionTypeProvider)
    {
        _serviceProvider = serviceProvider;
        _actionTypeProvider = actionTypeProvider;
    }

    public void Verify(ExistenceCheckerSetting setting)
    {
        if (setting is null)
        {
            throw new ArgumentNullException(nameof(setting));
        }

        if (!setting.CheckMatchingHandlers && !setting.CheckExistingPolicies)
        {
            return;
        }

        var messageTypes = _actionTypeProvider.GetMessageActionTypes();
        var requestTypes = _actionTypeProvider.GetRequestActionTypes();

        VerifyMessages(messageTypes, setting);
        VerifyRequests(requestTypes, setting);
        if (_errors.Any())
        {
            throw MediatorException.CreateForInvalidHandlers(_errors.ToArray());
        }
    }

    private void VerifyMessages(IEnumerable<Type> queryTypes, ExistenceCheckerSetting setting)
    {
        foreach (var subject in queryTypes)
        {
            if (_alreadyVerified.Contains(subject))
            {
                continue;
            }

            var handlers = _serviceProvider.GetMessageHandlers(subject).ToArray();
            if (setting.CheckMatchingHandlers)
            {
                VerifyHandlerCount(handlers, subject);
            }

            if (setting.CheckExistingPolicies)
            {
                VerifyPolicies(handlers, subject, setting.IgnoredPolicyChecks);
            }

            _alreadyVerified.Add(subject);
        }
    }

    private void VerifyRequests(IEnumerable<Type> types, ExistenceCheckerSetting setting)
    {
        foreach (var subject in types)
        {
            if (_alreadyVerified.Contains(subject))
            {
                continue;
            }

            var resultType = RequestGenericHelpers.GetRequestResultType(subject);
            var handlers = _serviceProvider.GetRequestHandlers(subject, resultType);
            if (setting.CheckMatchingHandlers)
            {
                VerifyHandlerCount(handlers, subject);
            }

            if (setting.CheckExistingPolicies)
            {
                VerifyPolicies(handlers, subject, setting.IgnoredPolicyChecks);
            }

            _alreadyVerified.Add(subject);
        }
    }

    private void VerifyPolicies(object[] handlers, Type subject, HashSet<Type> ignoredSubjects)
    {
        if (ignoredSubjects.Contains(subject))
        {
            return;
        }

        if (PolicyResolver.HasActionPolicies(subject, handlers))
        {
            return;
        }

        _errors.Add(AuthorizationException.NoAuthorization(subject.ToString()).Message);
    }

    private void VerifyHandlerCount(object[] handlers, Type subject)
    {
        if (handlers.Count() == 0)
        {
            if (subject.GetCustomAttributes<NoHandlerAttribute>(true).Any())
            {
                return;
            }

            _errors.Add(MediatorExecutionException.CreateForNoHandler(subject, null).Message);
        }

        var anyIsSequence = false;
        var anyIsConcurrent = false;
        var anyIsSingle = false;
        foreach (var handler in handlers)
        {
            var isSequence = handler is ISequenceHandler;
            var isConcurrent = handler is IConcurrentHandler;
            var isSingle = !isSequence && !isConcurrent;
            anyIsSequence = anyIsSequence || isSequence;
            anyIsConcurrent = anyIsConcurrent || isConcurrent;
            anyIsSingle = anyIsSingle || isSingle;
        }

        if ((anyIsConcurrent && anyIsSequence)
            || (anyIsConcurrent && anyIsSingle)
            || (anyIsSequence && anyIsSingle))
        {
            _errors.Add(MediatorException.CreateForCanNotCombineHandlers(subject, handlers).Message);
        }
        else if (anyIsSingle && handlers.Length > 1)
        {
            _errors.Add(MediatorException.CreateForDuplicateHandlers(subject, handlers).Message);
        }
    }
}