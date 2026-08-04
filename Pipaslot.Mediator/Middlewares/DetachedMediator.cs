using Pipaslot.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Stands in for <see cref="MediatorContext.Mediator"/> of a context created outside a mediator execution.
/// Every call throws, so code under test that makes a nested mediator call reports the missing test double
/// instead of throwing <see cref="System.NullReferenceException"/>.
/// </summary>
internal sealed class DetachedMediator : IMediator
{
    internal static readonly DetachedMediator Instance = new();

    private DetachedMediator()
    {
    }

    public Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
    {
        throw MediatorException.CreateForContextWithoutMediator(request);
    }

    public Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default)
    {
        throw MediatorException.CreateForContextWithoutMediator(message);
    }

    public Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
    {
        throw MediatorException.CreateForContextWithoutMediator(request);
    }

    public Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default)
    {
        throw MediatorException.CreateForContextWithoutMediator(message);
    }
}
