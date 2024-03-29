﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pipaslot.Mediator
{
    internal class MediatorContextAccessor : IMediatorContextAccessor, INotificationProvider
    {
        private static AsyncLocal<Stack<MediatorContext>> _asyncLocal = new();
        private readonly IServiceProvider _serviceProvider;

        public MediatorContextAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public MediatorContext? Context
        {
            get => _asyncLocal.Value?.Peek();
        }

        [Obsolete("Use Context instead")]
        public MediatorContext? MediatorContext
        {
            get => Context;
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
            if(Context is null)
            {
                // Notification provider is called independently of the mediator
                var messageReceiver = _serviceProvider.GetService<NotificationReceiverMiddleware>();
                if (messageReceiver != null)
                {
                    messageReceiver.SendNotifications(notification);
                }
            }
            else
            {
                Context.AddResult(notification);
            }            
        }
    }
}
