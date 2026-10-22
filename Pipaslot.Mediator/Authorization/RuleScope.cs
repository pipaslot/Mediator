namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// How long the outcome of a <see cref="Rule"/> stays valid, which decides whether a client may cache an authorization answer.
/// </summary>
public enum RuleScope
{
    /// <summary>
    /// Rule outcome is changing during the time and may be caluculated from model state.
    /// </summary>
    State = 0,

    /// <summary>
    /// Rule outcope wont change during user session. Can be applied caching techniques.
    /// </summary>
    Identity = 1
}