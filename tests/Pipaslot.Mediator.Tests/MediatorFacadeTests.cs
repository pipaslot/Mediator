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
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IMediatorContextAccessor _contextAccessor = Substitute.For<IMediatorContextAccessor>();
    private readonly INotificationProvider _notificationProvider = Substitute.For<INotificationProvider>();

    private MediatorFacade CreateSut() => new(_mediator, _contextAccessor, _notificationProvider);

    [Fact]
    public void Context_ReturnsContextFromContextAccessor()
    {
        var context = CreateContext();
        _contextAccessor.Context.Returns(context);

        var result = CreateSut().Context;

        Assert.Same(context, result);
    }

    [Fact]
    public void ContextStack_ReturnsContextStackFromContextAccessor()
    {
        var stack = new List<MediatorContext> { CreateContext() };
        _contextAccessor.ContextStack.Returns(stack);

        var result = CreateSut().ContextStack;

        Assert.Same(stack, result);
    }

    [Fact]
    public void AddNotification_Notification_DelegatesToNotificationProvider()
    {
        var notification = new Notification { Content = "Text" };

        CreateSut().AddNotification(notification);

        _notificationProvider.Received(1).Add(notification);
    }

    [Fact]
    public async Task Dispatch_Message_DelegatesToMediatorAndReturnsItsResponse()
    {
        var message = Substitute.For<IMediatorAction>();
        using var cts = new CancellationTokenSource();
        var response = Substitute.For<IMediatorResponse>();
        _mediator.Dispatch(message, cts.Token).Returns(response);

        var result = await CreateSut().Dispatch(message, cts.Token);

        Assert.Same(response, result);
    }

    [Fact]
    public async Task DispatchUnhandled_Message_DelegatesToMediator()
    {
        var message = Substitute.For<IMediatorAction>();
        using var cts = new CancellationTokenSource();

        await CreateSut().DispatchUnhandled(message, cts.Token);

        await _mediator.Received(1).DispatchUnhandled(message, cts.Token);
    }

    [Fact]
    public async Task Execute_Request_DelegatesToMediatorAndReturnsItsResponse()
    {
        var request = Substitute.For<IMediatorAction<string>>();
        using var cts = new CancellationTokenSource();
        var response = Substitute.For<IMediatorResponse<string>>();
        _mediator.Execute(request, cts.Token).Returns(response);

        var result = await CreateSut().Execute(request, cts.Token);

        Assert.Same(response, result);
    }

    [Fact]
    public async Task ExecuteUnhandled_Request_DelegatesToMediatorAndReturnsItsResult()
    {
        var request = Substitute.For<IMediatorAction<string>>();
        using var cts = new CancellationTokenSource();
        _mediator.ExecuteUnhandled(request, cts.Token).Returns("data");

        var result = await CreateSut().ExecuteUnhandled(request, cts.Token);

        Assert.Equal("data", result);
    }

    private static MediatorContext CreateContext()
    {
        return new MediatorContext(
            Substitute.For<IMediator>(),
            Substitute.For<IMediatorContextAccessor>(),
            Substitute.For<IServiceProvider>(),
            new ReflectionCache(),
            Substitute.For<IMediatorAction>(),
            CancellationToken.None,
            null,
            null);
    }
}
