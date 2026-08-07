using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Thrown by the authorization middleware when an action cannot be authorized at all.
/// </summary>
/// <remarks>
/// A configuration problem, not a refused user: either the action's handler declares no policy (neither an
/// <see cref="IHandlerAuthorization{TAction}"/> implementation nor a policy attribute), or only some of the handlers of a
/// multi-handler action declare one - authorization is all-or-nothing across handlers. Mark an action that genuinely needs
/// no check with <see cref="AnonymousPolicyAttribute"/> rather than leaving it undeclared, so that a forgotten policy
/// keeps failing loudly. A user failing a declared policy produces <see cref="AuthorizationRuleNotMetException"/> instead.
/// </remarks>
public class AuthorizationException(AuthorizationExceptionTypes type, string message) : Exception(message)
{
    /// <summary>
    /// Which authorization failure occurred - a missing policy, an inconsistently authorized handler set, or, for
    /// <see cref="AuthorizationRuleNotMetException"/>, a rule the current user did not meet.
    /// </summary>
    public AuthorizationExceptionTypes Type { get; } = type;

    internal static AuthorizationException NoAuthorization(string actionIdentifier)
    {
        return new AuthorizationException(AuthorizationExceptionTypes.NoAuthorization,
            $"Authorization policies are missing for action {actionIdentifier}");
    }

    internal static AuthorizationException UnauthorizedHandler(IEnumerable<object> handlers)
    {
        var handlerNames = string.Join(", ", handlers.Select(h => h.GetType().FullName));
        return new AuthorizationException(AuthorizationExceptionTypes.UnauthorizedHandler,
            $"All action handlers or no one have to provide authorization policies. These handlers did not have policies: [{handlerNames}]");
    }
}