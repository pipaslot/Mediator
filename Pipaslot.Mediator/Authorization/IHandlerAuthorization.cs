namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Declares the authorization policy guarding an action. Implemented by the action's handler class alongside its
/// <c>Handle</c> method, and evaluated by the authorization middleware before the handler runs.
/// </summary>
/// <remarks>
/// Independent of ASP.NET Core's authorization: the returned <see cref="IPolicy"/> is a tree of <see cref="Rule"/>
/// values that resolves to allow/deny/unavailable and can be rendered back as a human-readable reason, which is how the
/// client learns why an action was refused. Because the policy is a value rather than an executed check, it can also be
/// resolved without running the action - that is what lets a UI hide a button it may not use.
/// <para>
/// Use <see cref="IHandlerAuthorizationAsync{TAction}"/> instead when deciding requires I/O. For the common cases prefer
/// the attributes on the action - <see cref="AnonymousPolicyAttribute"/>, <see cref="AuthenticatedPolicyAttribute"/>,
/// <see cref="RolePolicyAttribute"/> - and reach for this interface when the decision depends on the action's data.
/// Returning null throws <see cref="MediatorException"/>; to deny, return <see cref="Rule.Deny"/> with a reason.
/// See docs/wiki/7.-Authorization.md.
/// </para>
/// </remarks>
/// <typeparam name="TAction">Action type this policy guards</typeparam>
public interface IHandlerAuthorization<TAction> : IHandlerAuthorizationMarker
{
    /// <summary>
    /// Builds the policy for <paramref name="action"/>. Called before the handler, and also on its own when a client asks
    /// whether the action would be permitted.
    /// </summary>
    public IPolicy Authorize(TAction action);
}
