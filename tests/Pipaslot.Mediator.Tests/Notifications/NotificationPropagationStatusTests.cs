using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Notifications;

/// <summary>
/// An error-typed <see cref="Notification"/> added through <see cref="INotificationProvider"/>/<see cref="IMediatorFacade.AddNotification"/>
/// (directly on <see cref="MediatorContext.AddResult"/>) never flips <see cref="ExecutionStatus"/> by itself - neither when it is
/// forwarded from a nested context nor when it is added locally on the root context. Only
/// <see cref="MediatorContextExtensions.AddError(MediatorContext, string, bool)"/>/<see cref="MediatorContextExtensions.AddErrors"/>
/// fail the action (see Todos/1.5.2-analysis-open-design-questions.md, "narrow the coupling").
/// </summary>
public class NotificationPropagationStatusTests
{
    // Depth 0 is the local case (the notification is added directly on the root/only context); depth 1 and 2 are
    // the forwarded case (a nested call several levels down adds it, and it propagates up). Both must leave the
    // root Status untouched.
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task ErrorNotification_ViaProvider_NeverFailsStatus(int depth)
    {
        var sut = Factory.CreateConfiguredMediator();
        var res = await sut.Dispatch(new ErrorNotifyingAction(depth, StopPropagation: false));

        Assert.True(res.Success);
        Assert.Contains(res.Results, r => r is Notification n && n.Type == NotificationType.Error);
    }

    [Fact]
    public async Task LocalAddError_AtRoot_FailsStatus()
    {
        var sut = Factory.CreateConfiguredMediator();
        var res = await sut.Dispatch(new AddErrorAction());

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
    public async Task DispatchUnhandled_StillThrowsForItsOwnLocalAddError()
    {
        var sut = Factory.CreateConfiguredMediator();

        await Assert.ThrowsAsync<MediatorUnhandledErrorException>(async () =>
            await sut.DispatchUnhandled(new AddErrorAction()));
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
    /// Recurses <paramref name="Depth"/> times before adding an error notification, so the theory above can vary
    /// how many nested contexts the notification propagates through.
    /// </summary>
    private record ErrorNotifyingAction(int Depth, bool StopPropagation) : IMediatorAction;

    private record AddErrorAction(bool StopPropagation = false) : IMediatorAction;

    private record AddErrorActionHandler(IMediatorFacade Facade) : IMediatorHandler<AddErrorAction>
    {
        public Task Handle(AddErrorAction action, CancellationToken cancellationToken)
        {
            Facade.Context!.AddError("boom", action.StopPropagation);
            return Task.CompletedTask;
        }
    }

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
            var nestedResult = await Facade.Dispatch(new AddErrorAction(StopPropagation: true), cancellationToken);
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
