using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared
{
    [AnonymousPolicy] ///Isalways anonymous as it is protected by DirectHttpCallProtectionMiddleware 
    public abstract class BaseInternalRequest<T> : IInternalRequest<T>
    {

    }
    /// <summary>
    /// Request which we want to protect agains direct calls from API as it may return sensitive data available on backed but forbidden to be transfered from the backend to frontend.
    /// </summary>
    public interface IInternalRequest<out T> : IMediatorAction<T>, IInternalRequest
    {
    }

    /// <summary>
    /// Marker interface used in mediator configuration only
    /// </summary>
    public interface IInternalRequest { }
}
