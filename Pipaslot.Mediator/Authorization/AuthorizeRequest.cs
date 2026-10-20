using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Asks whether the current user may execute <see cref="Action"/>, without executing it.
/// </summary>
/// <remarks>
/// Prefer the <see cref="MediatorExtensions.Authorize"/> and <see cref="MediatorExtensions.IsAuthorized"/> extensions over
/// sending it directly. Any caller may send it, and it runs in its own pipeline, which
/// <see cref="MediatorConfiguratorExtensions.AddPipelineForAuthorizationRequest"/> replaces.
/// </remarks>
/// <param name="action">Action to evaluate; it is not executed.</param>
[AnonymousPolicy]
public class AuthorizeRequest(IMediatorAction action) : IMediatorAction<AuthorizeRequestResponse>
{
    /// <summary>
    /// Action to evaluate. The policies of its handlers take part as well, so the answer matches what executing it would do.
    /// </summary>
    public IMediatorAction Action { get; set; } = action;
}