using System.Security.Claims;

namespace Pipaslot.Mediator.Authorization
{
    public interface IClaimPrincipalAccessor
    {
        ClaimsPrincipal? Principal { get; }
    }
}
