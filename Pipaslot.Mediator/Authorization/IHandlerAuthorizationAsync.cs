using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Asynchronous variant of <see cref="IHandlerAuthorization{TAction}"/>, for when building the policy requires I/O such as a
/// database lookup.
/// </summary>
/// <remarks>
/// Same contract as the synchronous variant: implemented by the handler class and evaluated before the handler runs;
/// returning null throws <see cref="MediatorException"/>. A handler implementing both contributes both policies, combined
/// with AND. Prefer the policy attributes when the decision does not depend on the action's data.
/// </remarks>
/// <typeparam name="TAction">Action type this policy guards</typeparam>
public interface IHandlerAuthorizationAsync<TAction> : IHandlerAuthorizationMarker
{
    /// <summary>
    /// Builds the policy for <paramref name="action"/>. Called before the handler, and also on its own when a client asks
    /// whether the action would be permitted.
    /// </summary>
    public Task<IPolicy> AuthorizeAsync(TAction action, CancellationToken cancellationToken);
}