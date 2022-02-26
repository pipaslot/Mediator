using System;
using System.Collections.Generic;
using System.Threading;

namespace Pipaslot.Mediator.Middlewares
{
    internal class MediatorContextAccessor : IMediatorContextAccessor
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
    }
}
