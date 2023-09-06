﻿using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator
{
    public interface IMediatorContextAccessor
    {
        /// <summary>
        /// Returns context of actual executed action
        /// </summary>
        [Obsolete("Use Context instead.")]
        MediatorContext? MediatorContext { get; }

        /// <summary>
        /// Returns context of actual executed action
        /// </summary>
        MediatorContext? Context { get; }

        /// <summary>
        /// All action context executed recursively
        /// First is the root action
        /// Last is the actual action
        /// </summary>
        IReadOnlyCollection<MediatorContext> ContextStack { get; }
    }
}
