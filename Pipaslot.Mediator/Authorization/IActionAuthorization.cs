namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Declares the authorization policy on the action itself, for a rule that holds whichever handler runs.
/// </summary>
/// <remarks>
/// Combined with AND with the policies declared by the handlers and by policy attributes. Use the attributes for a
/// condition that does not depend on the action's data, and <see cref="IHandlerAuthorization{TAction}"/> or
/// <see cref="IHandlerAuthorizationAsync{TAction}"/> when building the policy needs services injected into the handler.
/// </remarks>
public interface IActionAuthorization
{
    /// <summary>
    /// Builds the policy for this action instance. Called before any handler runs, and on its own when a client asks
    /// whether the action would be permitted.
    /// </summary>
    public IPolicy Authorize();
}