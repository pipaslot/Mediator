using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization;

public interface IHandlerAuthorizationAsync<TAction> : IHandlerAuthorizationMarker
{
    public Task<IPolicy> AuthorizeAsync(TAction action, CancellationToken cancellationToken);
}