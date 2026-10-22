using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Supplies the current user to <see cref="IdentityPolicy"/> and to the policy attributes built on it.
/// </summary>
/// <remarks>
/// Replace the registration to read the user from your own authentication. <c>AddMediatorServer</c> already registers an
/// implementation reading <c>HttpContext.User</c>; the default from <c>AddMediator</c> is <see cref="ClaimPrincipalAccessor"/>.
/// </remarks>
public interface IClaimPrincipalAccessor
{
    /// <summary>
    /// The current user, or null when there is none - an identity policy then treats the caller as unauthenticated and
    /// without claims.
    /// </summary>
    ClaimsPrincipal? Principal { get; }
}