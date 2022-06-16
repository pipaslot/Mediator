using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Authorization;
using System.Security.Claims;

namespace Pipaslot.Mediator.Http
{
    public class ClaimPrincipalAccessor : IClaimPrincipalAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimPrincipalAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
    }
}
