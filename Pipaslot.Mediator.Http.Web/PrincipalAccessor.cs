using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Authorization;
using System.Security.Claims;

namespace Pipaslot.Mediator.Http.Web;

public class ClaimPrincipalAccessor(IHttpContextAccessor httpContextAccessor) : IClaimPrincipalAccessor
{
    public ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
}