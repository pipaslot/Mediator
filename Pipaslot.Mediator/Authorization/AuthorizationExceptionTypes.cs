namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Which authorization failure an <see cref="AuthorizationException"/> reports, read from <see cref="AuthorizationException.Type"/>.
/// </summary>
public enum AuthorizationExceptionTypes
{
    /// <summary>
    /// Default value; the library itself never raises it.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Neither the action nor any of its handlers declares a policy. Actions are not public by default - mark a public one
    /// with <see cref="AnonymousPolicyAttribute"/>.
    /// </summary>
    NoAuthorization = 1,

    /// <summary>
    /// Some handlers of the action declare a policy and others do not. Either all handlers of an action declare one, or none.
    /// </summary>
    UnauthorizedHandler = 2,

    /// <summary>
    /// The current user did not satisfy the policy; raised as <see cref="AuthorizationRuleNotMetException"/>.
    /// </summary>
    RuleNotMet = 3
}