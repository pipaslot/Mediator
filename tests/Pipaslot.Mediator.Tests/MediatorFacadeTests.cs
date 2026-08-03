using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// <see cref="MediatorFacade"/> is a pure delegator combining <see cref="IMediator"/>,
/// <see cref="IMediatorContextAccessor"/>, and <see cref="INotificationProvider"/> behind one interface. These
/// tests verify each member forwards to the matching collaborator, since none of the other test classes construct
/// <see cref="MediatorFacade"/> itself as the SUT (it is only ever consumed as a handler dependency).
/// </summary>
public class MediatorFacadeTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IMediatorContextAccessor> _contextAccessor = new();
    private readonly Mock<INotificationProvider> _notificationProvider = new();

    private MediatorFacade CreateSut() => new(_mediator.Object, _contextAccessor.Object, _notificationProvider.Object);

    [Fact]
    public void Context_ReturnsContextFromContextAccessor()
    {
        var context = CreateContext();
        _contextAccessor.Setup(a => a.Context).Returns(context);

        var result = CreateSut().Context;

        Assert.Same(context, result);
    }

    [Fact]
    public void ContextStack_ReturnsContextStackFromContextAccessor()
    {
        var stack = new List<MediatorContext> { CreateContext() };
        _contextAccessor.Setup(a => a.ContextStack).Returns(stack);

        var result = CreateSut().ContextStack;

        Assert.Same(stack, result);
    }

    [Fact]
    public void AddNotification_Notification_DelegatesToNotificationProvider()
    {
        var notification = new Notification { Content = "Text" };

        CreateSut().AddNotification(notification);

        _notificationProvider.Verify(p => p.Add(notification), Times.Once);
    }

    [Fact]
    public async Task Dispatch_Message_DelegatesToMediatorAndReturnsItsResponse()
    {
        var message = new Mock<IMediatorAction>().Object;
        using var cts = new CancellationTokenSource();
        var response = new Mock<IMediatorResponse>().Object;
        _mediator.Setup(m => m.Dispatch(message, cts.Token)).ReturnsAsync(response);

        var result = await CreateSut().Dispatch(message, cts.Token);

        Assert.Same(response, result);
    }

    [Fact]
    public async Task DispatchUnhandled_Message_DelegatesToMediator()
    {
        var message = new Mock<IMediatorAction>().Object;
        using var cts = new CancellationTokenSource();

        await CreateSut().DispatchUnhandled(message, cts.Token);

        _mediator.Verify(m => m.DispatchUnhandled(message, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Execute_Request_DelegatesToMediatorAndReturnsItsResponse()
    {
        var request = new Mock<IMediatorAction<string>>().Object;
        using var cts = new CancellationTokenSource();
        var response = new Mock<IMediatorResponse<string>>().Object;
        _mediator.Setup(m => m.Execute(request, cts.Token)).ReturnsAsync(response);

        var result = await CreateSut().Execute(request, cts.Token);

        Assert.Same(response, result);
    }

    [Fact]
    public async Task ExecuteUnhandled_Request_DelegatesToMediatorAndReturnsItsResult()
    {
        var request = new Mock<IMediatorAction<string>>().Object;
        using var cts = new CancellationTokenSource();
        _mediator.Setup(m => m.ExecuteUnhandled(request, cts.Token)).ReturnsAsync("data");

        var result = await CreateSut().ExecuteUnhandled(request, cts.Token);

        Assert.Equal("data", result);
    }

    private static MediatorContext CreateContext()
    {
        return new MediatorContext(
            new Mock<IMediator>().Object,
            new Mock<IMediatorContextAccessor>().Object,
            new Mock<IServiceProvider>().Object,
            new ReflectionCache(),
            new Mock<IMediatorAction>().Object,
            CancellationToken.None,
            null,
            null);
    }
}
