using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Notifications;

/// <summary>
/// A nested action's error <see cref="Notification"/>, once forwarded into the parent context, must not by
/// itself flip the parent's <see cref="ExecutionStatus"/> to Failed.
/// </summary>
public class NotificationPropagationStatusTests
{
    // Depth 1 and 2 are the propagation scenario the report describes (root never touches Status/AddNotification
    // itself; only a nested call several levels down does). Depth 0 is deliberately excluded here and covered
    // instead by LocalErrorNotification_AtRoot_StillFailsStatus, because at depth 0 the notification is added
    // directly on the root/only context - that is a *local* failure, not a forwarded one.
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task ForwardedErrorNotification_DoesNotFailRootStatus(int depth)
    {
        var sut = Factory.CreateConfiguredMediator();
        var res = await sut.Dispatch(new ErrorNotifyingAction(depth, StopPropagation: false));

        Assert.True(res.Success);
        Assert.Contains(res.Results, r => r is Notification n && n.Type == NotificationType.Error);
    }

    [Fact]
    public async Task LocalErrorNotification_AtRoot_StillFailsStatus()
    {
        var sut = Factory.CreateConfiguredMediator();
        var res = await sut.Dispatch(new ErrorNotifyingAction(0, StopPropagation: false));

        Assert.False(res.Success);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task StopPropagation_KeepsRootSucceededAndHidesNotification(int depth)
    {
        var sut = Factory.CreateConfiguredMediator();
        var res = await sut.Dispatch(new ErrorNotifyingAction(depth, StopPropagation: true));

        Assert.True(res.Success);
        Assert.DoesNotContain(res.Results, r => r is Notification);
    }

    [Fact]
    public async Task ParentExplicitlyReactingToNestedFailure_CanStillFailItself()
    {
        var sut = Factory.CreateConfiguredMediator();
        var res = await sut.Dispatch(new ReactingParentAction());

        Assert.False(res.Success);
        Assert.Contains(res.Results, r => r is Notification n && n.Content.Contains("explicitly failing"));
    }

    [Fact]
    public async Task DispatchUnhandled_StillThrowsForItsOwnLocalError()
    {
        var sut = Factory.CreateConfiguredMediator();

        await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            await sut.DispatchUnhandled(new ErrorNotifyingAction(0, StopPropagation: false)));
    }

    [Theory]
    [InlineData(NotificationType.Success, 0)]
    [InlineData(NotificationType.Information, 0)]
    [InlineData(NotificationType.Warning, 0)]
    [InlineData(NotificationType.Success, 1)]
    [InlineData(NotificationType.Information, 1)]
    [InlineData(NotificationType.Warning, 1)]
    public async Task NonErrorNotificationTypes_NeverFailStatus(NotificationType type, int depth)
    {
        var sut = Factory.CreateConfiguredMediator();
        var res = await sut.Dispatch(new TypedNotifyingAction(depth, type));

        Assert.True(res.Success);
    }

    /// <summary>
    /// Recurses <paramref name="Depth"/> times before adding an error notification, reproducing
    /// Todos/1.3's <c>ErrorNotifyingAction</c>/<c>Handler</c> pair.
    /// </summary>
    private record ErrorNotifyingAction(int Depth, bool StopPropagation) : IMediatorAction;

    private record ErrorNotifyingActionHandler(IMediatorFacade Facade) : IMediatorHandler<ErrorNotifyingAction>
    {
        public async Task Handle(ErrorNotifyingAction action, CancellationToken cancellationToken)
        {
            if (action.Depth > 0)
            {
                await Facade.Dispatch(action with { Depth = action.Depth - 1 }, cancellationToken);
            }
            else
            {
                Facade.AddNotification(new Notification
                {
                    Content = "boom", Type = NotificationType.Error, StopPropagation = action.StopPropagation
                });
            }
        }
    }

    private record ReactingParentAction : IMediatorAction;

    private record ReactingParentActionHandler(IMediatorFacade Facade) : IMediatorHandler<ReactingParentAction>
    {
        public async Task Handle(ReactingParentAction action, CancellationToken cancellationToken)
        {
            // StopPropagation:true isolates the mechanism under test: the parent's failure must come only from
            // its own explicit AddError below, not from the nested notification also auto-propagating.
            var nestedResult = await Facade.Dispatch(new ErrorNotifyingAction(0, StopPropagation: true), cancellationToken);
            if (!nestedResult.Success)
            {
                Facade.Context!.AddError("Parent explicitly failing because a nested action failed");
            }
        }
    }

    private record TypedNotifyingAction(int Depth, NotificationType Type) : IMediatorAction;

    private record TypedNotifyingActionHandler(IMediatorFacade Facade) : IMediatorHandler<TypedNotifyingAction>
    {
        public async Task Handle(TypedNotifyingAction action, CancellationToken cancellationToken)
        {
            if (action.Depth > 0)
            {
                await Facade.Dispatch(action with { Depth = action.Depth - 1 }, cancellationToken);
            }
            else
            {
                Facade.AddNotification(new Notification { Content = "note", Type = action.Type });
            }
        }
    }
}
