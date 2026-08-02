using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// A handler that makes a nested <see cref="IMediator.DispatchUnhandled"/>/<see cref="IMediator.ExecuteUnhandled"/>
/// call to another handler which either "handles" its own failure by recording a translated notification (via
/// <c>AddError</c>) instead of throwing, or throws a business exception directly. Because the call is nested, a
/// recorded notification propagates up to the parent context via <see cref="NotificationPropagationMiddleware"/>
/// before <c>*Unhandled</c> throws for the same failure.
/// Introduced in version 9.0.0: the parent's own unguarded <c>Dispatch</c>/<c>Execute</c> boundary no longer re-adds a
/// duplicate generic wrapper message for a <see cref="MediatorUnhandledErrorException"/>, and resolves a typed
/// <see cref="IMediatorExceptionHandler{TException}"/> for the original exception type when the inner failure
/// reaches it as a real exception rather than a swallowed one.
/// Kept in its own file (like  <c>Notifications/NotificationPropagationTests.cs</c>) since these scenarios span
/// two nested contexts and have no natural home in a single-context E2E test.
/// </summary>
public class Mediator_NestedUnhandledCollapseTests
{
    /// <summary>
    /// The outer handler does not catch the exception from its nested call (mirrors a real-world gap), so it
    /// bubbles out to the outer <c>Dispatch</c>. Before Unit 4, the outer catch additionally added
    /// <c>MediatorUnhandledErrorException</c>'s wrapper message as a second, duplicate notification on top of the
    /// one already propagated by <see cref="NotificationPropagationMiddleware"/> - Unit 4 removes that duplication.
    /// </summary>
    [Fact]
    public async Task Dispatch_NestedDispatchUnhandledFailsWithoutThrow_ParentGetsExactlyThePropagatedNotification()
    {
        var sut = Factory.CreateConfiguredMediator();

        var result = await sut.Dispatch(new OuterAction());

        Assert.False(result.Success);
        var notifications = result.Results.OfType<Notification>().ToArray();

        var notification = Assert.Single(notifications);
        Assert.Equal(InnerActionHandler.TranslatedMessage, notification.Content);
    }

    /// <summary>
    /// Same scenario as above, observed through the boundary's logging instead of Results: the legacy
    /// <c>MediatorUnhandledErrorException</c> fallback logs at Warning (not Error) with no default escalation,
    /// because the library cannot tell an expected/already-translated failure from a genuine bug at this point.
    /// </summary>
    [Fact]
    public async Task Dispatch_NestedDispatchUnhandledFailsWithoutThrow_LogsWarningNotError()
    {
        var (sut, logger) = Factory.CreateConfiguredMediatorWithLogger();

        await sut.Dispatch(new OuterAction());

        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Warning);
        Assert.IsType<MediatorUnhandledErrorException>(entry.Exception);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);
    }

    /// <summary>
    /// Verifies that when a nested <c>ExecuteUnhandled</c> failure reaches the outer <c>Dispatch</c> as the original
    /// exception type, the outer boundary resolves the matching typed <see cref="IMediatorExceptionHandler{TException}"/>
    /// instead of the generic <see cref="MediatorUnhandledErrorException"/> wrapper.
    /// </summary>
    [Fact]
    public async Task Dispatch_NestedExecuteUnhandledRecordsAndRethrowsOriginalType_OuterResolvesHandlerForOriginalType()
    {
        var sut = Factory.CreateConfiguredMediator(c =>
        {
            // Scoped to the inner action only - a global Use<> would also intercept the outer action's own dispatch.
            c.UseWhenAction<RecordingInnerAction, RecordBusinessExceptionMiddleware>();
            c.AddExceptionHandler<BusinessExceptionHandler>();
        });

        var result = await sut.Dispatch(new RecordingOuterAction());

        Assert.False(result.Success);
        Assert.Equal(BusinessExceptionHandler.TranslatedMessage, result.GetErrorMessage());
    }

        /// <summary>
    /// End\-to\-end acceptance test for a nested failure flow: an inner <c>ExecuteUnhandled</c> handler throws a
    /// business exception directly (without consumer catch\-and\-swallow middleware), and a typed handler is
    /// registered for that exception. The outer <c>Dispatch</c> boundary must return exactly one user message \-
    /// the typed translation \- without duplicated technical wrapper text, and must not escalate to an Error\-level
    /// log (Warning at most), because the failure is intentionally mapped and translated.
    /// </summary>
    [Fact]
    public async Task Dispatch_NestedExecuteUnhandledHandlerThrowsMappedBusinessException_ExactlyOneTranslatedMessageNoErrorLog()
    {
        var (sut, logger) = Factory.CreateConfiguredMediatorWithLogger(c => c.AddExceptionHandler<BusinessExceptionHandler>());

        var result = await sut.Dispatch(new ThrowingOuterAction());

        Assert.False(result.Success);
        var notification = Assert.Single(result.Results.OfType<Notification>());
        Assert.Equal(BusinessExceptionHandler.TranslatedMessage, notification.Content);
        Assert.DoesNotContain(BusinessException.OriginalMessage, notification.Content);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);
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

    private class BusinessException(string message) : Exception(message)
    {
        public const string OriginalMessage = "technical failure detail that must not reach the client";
    }

    private class BusinessExceptionHandler : IMediatorExceptionHandler<BusinessException>
    {
        public const string TranslatedMessage = "A business rule prevented this action.";

        public Task Handle(BusinessException exception, IMediatorExceptionContext context)
        {
            context.SetHandled(TranslatedMessage);
            return Task.CompletedTask;
        }
    }

    private record RecordingOuterAction : IMediatorAction;

    private record RecordingOuterActionHandler(IMediatorFacade Facade) : IMediatorHandler<RecordingOuterAction>
    {
        public Task Handle(RecordingOuterAction action, CancellationToken cancellationToken)
        {
            // Deliberately not wrapped in try/catch - mirrors the real-world call site.
            return Facade.DispatchUnhandled(new RecordingInnerAction(), cancellationToken);
        }
    }

    private record RecordingInnerAction : IMediatorAction;

    private class RecordBusinessExceptionMiddleware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddException(new BusinessException(BusinessException.OriginalMessage));
            // Do not call next - mirrors a terminal validator middleware; the handler never runs.
            return Task.CompletedTask;
        }
    }

    private record ThrowingOuterAction : IMediatorAction;

    private record ThrowingOuterActionHandler(IMediatorFacade Facade) : IMediatorHandler<ThrowingOuterAction>
    {
        public Task Handle(ThrowingOuterAction action, CancellationToken cancellationToken)
        {
            // Deliberately not wrapped in try/catch - mirrors the real-world call site.
            return Facade.ExecuteUnhandled(new ThrowingInnerAction(), cancellationToken);
        }
    }

    private record ThrowingInnerAction : IRequest<ThrowingInnerAction.ResultDto>
    {
        public record ResultDto;
    }

    private class ThrowingInnerActionHandler : IRequestHandler<ThrowingInnerAction, ThrowingInnerAction.ResultDto>
    {
        public Task<ThrowingInnerAction.ResultDto> Handle(ThrowingInnerAction action, CancellationToken cancellationToken)
        {
            throw new BusinessException(BusinessException.OriginalMessage);
        }
    }
}
