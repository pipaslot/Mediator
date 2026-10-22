using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Default <see cref="IClaimPrincipalAccessor"/> registered by <c>AddMediator</c>, returning <see cref="ClaimsPrincipal.Current"/>.
/// </summary>
/// <remarks>
/// <c>AddMediatorServer</c> from Pipaslot.Mediator.Http removes this registration and replaces it with its own class of the same name,
/// <c>Pipaslot.Mediator.Http.ClaimPrincipalAccessor</c> (singleton), which reads <c>HttpContext.User</c>.
/// This class therefore applies with <c>AddMediator</c> and <c>AddMediatorClient</c>.
/// <see cref="ClaimsPrincipal.Current"/> returns <c>Thread.CurrentPrincipal</c>, which the library never sets - register
/// your own <see cref="IClaimPrincipalAccessor"/> wherever identity policies have to see the user.
/// </remarks>
public class ClaimPrincipalAccessor : IClaimPrincipalAccessor
{
    /// <inheritdoc/>
    public ClaimsPrincipal? Principal => ClaimsPrincipal.Current;
}