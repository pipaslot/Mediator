using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// A handler that makes a nested <see cref="IMediator.DispatchUnhandled"/> call to another handler which "handles"
/// its own failure by recording a translated notification (via <c>AddError</c>) instead of throwing. Because the
/// call is nested, that notification propagates up to the parent context - but the parent's own unguarded
/// <c>Dispatch</c> then also catches the exception <c>DispatchUnhandled</c> throws for the same failure and adds a
/// second, duplicate wrapper notification on top of the first. Kept in its own file (like
/// <c>Notifications/NotificationPropagationTests.cs</c>) since the scenario spans two nested contexts and has no
/// natural home in a single-context E2E test.
/// </summary>
public class Mediator_NestedUnhandledCollapseTests
{
    // TODO: once Dispatch/Execute stop re-adding a generic wrapper message for a failure a nested call already
    // translated, this scenario should produce exactly ONE notification - the propagated
    // InnerActionHandler.TranslatedMessage - not two. Replace the two Assert.Contains below with:
    //   Assert.Single(notifications);
    //   Assert.Equal(InnerActionHandler.TranslatedMessage, notifications[0].Content);
    /// <summary>
    /// The outer handler does not catch the exception from its nested call (mirrors a real-world gap), so it
    /// bubbles out to the outer <c>Dispatch</c>, whose own catch re-adds the wrapper message as a second
    /// notification alongside the one already propagated by <see cref="NotificationPropagationMiddleware"/>.
    /// </summary>
    [Fact]
    public async Task Dispatch_NestedDispatchUnhandledFailsWithoutThrow_ParentGetsBothPropagatedNotificationAndWrapperMessage()
    {
        var sut = Factory.CreateConfiguredMediator();

        var result = await sut.Dispatch(new OuterAction());

        Assert.False(result.Success);
        var notifications = result.Results.OfType<Notification>().ToArray();

        // The original, already-translated notification propagated up from the inner (nested) context.
        Assert.Contains(notifications, n => n.Content == InnerActionHandler.TranslatedMessage);

        // The outer Dispatch's own catch additionally added CreateForUnhandledError's wrapper message, which
        // embeds the same translated content inside a generic technical sentence - i.e. duplicated reporting.
        // This second notification is the redundant one a future fix removes - see the TODO above.
        Assert.Contains(notifications, n => n.Content != InnerActionHandler.TranslatedMessage && n.Content.Contains(InnerActionHandler.TranslatedMessage));
    }

    private record OuterAction : IMediatorAction;

    private record OuterActionHandler(IMediatorFacade Facade) : IMediatorHandler<OuterAction>
    {
        public Task Handle(OuterAction action, CancellationToken cancellationToken)
        {
            // Deliberately not wrapped in try/catch - mirrors the real-world call site that does not (yet)
            // catch MediatorExecutionException around a nested *Unhandled call.
            return Facade.DispatchUnhandled(new InnerAction(), cancellationToken);
        }
    }

    private record InnerAction : IMediatorAction;

    private record InnerActionHandler(IMediatorFacade Facade) : IMediatorHandler<InnerAction>
    {
        public const string TranslatedMessage = "translated business error";

        public Task Handle(InnerAction action, CancellationToken cancellationToken)
        {
            // Simulates a terminal error-handling middleware that already translated a caught exception into a
            // user-facing notification and intentionally does not rethrow.
            Facade.Context!.AddError(TranslatedMessage);
            return Task.CompletedTask;
        }
    }
}
