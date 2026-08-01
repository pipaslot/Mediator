using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// Characterization/regression tests for the Todos/1.3 error-handling redesign. Most of today's Dispatch/Execute/
/// DispatchUnhandled/ExecuteUnhandled exception-path behavior is already characterized by existing E2E tests:
/// handler-throws by <see cref="E2E.BasicFailing"/>, no-handler by <see cref="E2E.Nohandler"/>, middleware-sets-
/// Failed-without-throw by <see cref="E2E.NoHandlerAndErrorReturned"/>, and missing-result by
/// <see cref="E2E.ResultWasTakenFromTheContext"/> - those are the files Units 1-4 update in place as behavior
/// changes, rather than duplicating their assertions here. This class holds only what wasn't already covered
/// anywhere, plus whatever new API each unit introduces.
/// </summary>
public class Mediator_ErrorHandlingTests
{
    #region Nested ExecuteUnhandled/DispatchUnhandled collapse (Todos/1.0 regression)

    // TODO(Unit 3/4): once the outer Dispatch/Execute boundary stops doing a blind `AddError(e.Message)` and
    // instead special-cases MediatorUnhandledErrorException (per 1.2 §2.3/Unit 4 scope: "do not add e.Message
    // again - already-translated content is already in context.Results from the inner pipeline"), this scenario
    // must produce exactly ONE notification - the propagated InnerActionHandler.TranslatedMessage - with no
    // second, wrapper-text notification. When that lands, replace the two Assert.Contains below with:
    //   Assert.Single(notifications);
    //   Assert.Equal(InnerActionHandler.TranslatedMessage, notifications[0].Content);
    // and add a companion assertion that the outer boundary logged at Warning (not Error), per Unit 4's test list.
    /// <summary>
    /// Reproduces Todos/1.0: an outer handler makes a nested <see cref="IMediator.DispatchUnhandled"/> call whose
    /// inner pipeline "handles" its own failure by recording a translated notification via <c>AddError</c> without
    /// throwing (as a terminal error-handling middleware would). Because the inner call is nested,
    /// <see cref="NotificationPropagationMiddleware"/> forwards that notification to the parent context *before*
    /// DispatchUnhandled synthesizes and throws <see cref="MediatorExecutionException.CreateForUnhandledError"/> for
    /// the inner failure. The outer handler does not catch that exception (mirrors the real-world gap), so it
    /// bubbles out to the unguarded outer Dispatch, whose own catch re-adds the wrapper message as a second
    /// notification. Not covered anywhere else in the suite - keep this test and re-target its assertions per the
    /// TODO above rather than deleting it.
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
        // This second notification is the redundant one Unit 3/4 removes - see the TODO above.
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

    #endregion
}
