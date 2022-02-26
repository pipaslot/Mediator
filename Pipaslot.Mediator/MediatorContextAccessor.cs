using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pipaslot.Mediator
{
    internal class MediatorContextAccessor : IMediatorContextAccessor, INotificationProvider
    {
        private static AsyncLocal<Stack<MediatorContext>> _asyncLocal = new();

        public MediatorContext? MediatorContext
        {
            get => _asyncLocal.Value?.Peek();
        }

        public IReadOnlyCollection<MediatorContext> ContextStack => _asyncLocal.Value?.ToArray() ?? Array.Empty<MediatorContext>();

        public void Push(MediatorContext context)
        {
            var stack = _asyncLocal.Value ??= new();
            stack.Push(context);
        }

        public void Pop()
        {
            _asyncLocal.Value?.Pop();
        }

        public void Add(Notification notification)
        {
            _asyncLocal.Value?.FirstOrDefault()?.AddResult(notification);
        }
    }
}
