using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization
{
    public class ClaimPrincipalAccessor : IClaimPrincipalAccessor
    {
        public ClaimsPrincipal? Principal => ClaimsPrincipal.Current;
    }
}
