using Pipaslot.Mediator.Middlewares;
using System.Collections.Generic;

namespace Pipaslot.Mediator
{
    public interface IMediatorContextAccessor
    {
        /// <summary>
        /// Returns context of actual executed action
        /// </summary>
        MediatorContext? MediatorContext { get; }

        /// <summary>
        /// All action context executed recursively
        /// </summary>
        IReadOnlyCollection<MediatorContext> ContextStack { get; }
    }
}
