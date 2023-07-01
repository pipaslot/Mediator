using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator
{
    public class MediatorFacade : IMediatorFacade
    {
        private readonly IMediator _mediator;
        private readonly IMediatorContextAccessor _mediatorContextAccessor;
        private readonly INotificationProvider _notificationProvider;

        public MediatorFacade(IMediator mediator, IMediatorContextAccessor mediatorContextAccessor, INotificationProvider notificationProvider)
        {
            _mediator = mediator;
            _mediatorContextAccessor = mediatorContextAccessor;
            _notificationProvider = notificationProvider;
        }

        #region MediatorContextAccessor

        public MediatorContext? MediatorContext => _mediatorContextAccessor.MediatorContext;

        public IReadOnlyCollection<MediatorContext> ContextStack => _mediatorContextAccessor.ContextStack;


        #endregion

        #region Notification Provider
        public void AddNotification(Notification notification)
        {
            _notificationProvider.Add(notification);
        }

        #endregion

        #region Mediator 

        public Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            return _mediator.Dispatch(message, cancellationToken);
        }

        public Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            return _mediator.DispatchUnhandled(message, cancellationToken);
        }

        public Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            return _mediator.Execute(request, cancellationToken);
        }

        public Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            return _mediator.ExecuteUnhandled(request, cancellationToken);
        }

        #endregion
    }
}
