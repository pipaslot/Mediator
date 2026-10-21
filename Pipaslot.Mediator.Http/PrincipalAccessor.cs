using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Authorization;
using System.Security.Claims;

namespace Pipaslot.Mediator.Http;

/// <summary>
/// <see cref="IClaimPrincipalAccessor"/> returning <c>HttpContext.User</c> of the current HTTP request, or <c>null</c> outside of one.
/// </summary>
/// <remarks>
/// Registered as a singleton by <see cref="ServiceCollectionExtensions.AddMediatorServer(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
/// It has the same name as <see cref="Pipaslot.Mediator.Authorization.ClaimPrincipalAccessor"/> from the core library, which
/// <c>AddMediator</c> registers and which reads <see cref="ClaimsPrincipal.Current"/>; on the server this class is registered
/// afterwards and therefore the one that identity policies see. Requires <c>IHttpContextAccessor</c> in the service collection.
/// </remarks>
/// <param name="httpContextAccessor">Accessor of the current HTTP context.</param>
public class ClaimPrincipalAccessor(IHttpContextAccessor httpContextAccessor) : IClaimPrincipalAccessor
{
    /// <inheritdoc/>
    public ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
}