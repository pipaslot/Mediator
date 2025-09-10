using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading;

namespace Pipaslot.Mediator.Tests.Notifications;

public class NotificationReceiverMiddlewareTests
{

    [Test]
    public void RegisterINotificationReceiver()
    {
        var services = Factory.CreateServiceProvider(c => c.UseNotificationReceiver());
        var provider = services.GetService<INotificationReceiver>();
        await Assert.That(provider).IsNotNull();
    }

    [Test]
    public async Task AnyOtherObjectDoesNotFireEvent()
    {
        await InvokeMiddleware(c => { c.AddResult(new object()); }, false);
    }

    [Test]
    public async Task NotificationObjectFiresEvent()
    {
        await InvokeMiddleware(c => { c.AddResult(new Notification()); }, true);
    }

    [Test]
    public async Task ErrorMessageFiresEvent()
    {
        await InvokeMiddleware(c => { c.AddError("haha"); }, true);
    }

    private async Task InvokeMiddleware(Action<MediatorContext> setup, bool expected)
    {
        var received = false;
        var services = Factory.CreateServiceProvider(c => c.UseNotificationReceiver());
        var provider = services.GetRequiredService<INotificationReceiver>();
        provider.NotificationReceived += (a, b) =>
        {
            received = true;
        };
        var mediator = new Mock<IMediator>();
        var sut = services.GetRequiredService<NotificationReceiverMiddleware>();
        var mcaMock = new Mock<IMediatorContextAccessor>();
        var context = new MediatorContext(mediator.Object, mcaMock.Object, services, new ReflectionCache(), new NopMessage(), CancellationToken.None, null, null);
        setup(context);
        await sut.Invoke(context, c => Task.CompletedTask);
        await Assert.That(received).IsEqualTo(expected);
    }
}