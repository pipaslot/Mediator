using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests.Notifications
{
    public class NotificationReceiverMiddlewareTests
    {
        [Fact]
        public void INotificationReceiverIsNotReceiverdByDefault()
        {
            var services = Factory.CreateServiceProvider(c => { });
            var provider = services.GetService<INotificationReceiver>();
            Assert.Null(provider);
        }

        [Fact]
        public void RegisterINotificationReceiver()
        {
            var services = Factory.CreateServiceProvider(c => c.UseNotificationReceiver());
            var provider = services.GetService<INotificationReceiver>();
            Assert.NotNull(provider);
        }

        [Fact]
        public async Task AnyOtherObjectDoesNotFireEvent()
        {
            await InvokeMiddleware(c => { c.AddResult(new object()); }, false);
        }

        [Fact]
        public async Task NotificationObjectFiresEvent()
        {
            await InvokeMiddleware(c => { c.AddResult(new Notification()); }, true);
        }

        [Fact]
        public async Task ErrorMessageFiresEvent()
        {
            await InvokeMiddleware(c => { c.AddError("haha"); }, true);
        }

        private async Task InvokeMiddleware(Action<MediatorContext> setup, bool expected)
        {
            var received = false;
            var services = Factory.CreateServiceProvider(c => c.UseNotificationReceiver());
            var provider = services.GetService<INotificationReceiver>();
            provider.NotificationReceived += (a, b) =>
            {
                received = true;
            };
            var sut = services.GetService<NotificationReceiverMiddleware>();
            var context = new MediatorContext(new NopMessage(), CancellationToken.None);
            setup(context);
            await sut.Invoke(context, c => Task.CompletedTask);
            Assert.Equal(expected, received);
        }
    }
}
