using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// Covers the DispatchUnhandled/ExecuteUnhandled re-throw bridge built on top of <c>context.AddException</c>:
/// a single recorded exception is rethrown with its original type and stack trace instead of being
/// wrapped in <see cref="MediatorUnhandledErrorException"/>; multiple recorded exceptions are aggregated; the
/// recorded collection never auto-propagates to a parent context. The legacy AddError-only fallback (no
/// AddException call anywhere) is unchanged and already covered by <c>E2E.NoHandlerWithoutErrorTests</c>/
/// <c>E2E.NoHandlerAndErrorReturnedTests</c> - not duplicated here. <c>context.AddException</c>/<c>Exceptions</c>
/// themselves (status flip, no Notification, no Results entry) are covered directly on the context in
/// <c>Middlewares/MediatorContextTests.cs</c>.
/// </summary>
public class Mediator_ExceptionsRethrowBridgeTests
{
    [Fact]
    public async Task ExecuteUnhandled_SingleAddException_RethrowsOriginalTypeWithPreservedStackTrace()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RecordSingleExceptionMiddleware>());

        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteUnhandled(new SingleHandler.Request(true)));

        Assert.Equal(RecordSingleExceptionMiddleware.Message, ex.Message);
        Assert.Contains("ThrowOriginal", ex.StackTrace);
    }

    [Fact]
    public async Task DispatchUnhandled_SingleAddException_RethrowsOriginalTypeWithPreservedStackTrace()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RecordSingleExceptionMiddleware>());

        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.DispatchUnhandled(new SingleHandler.Message(true)));

        Assert.Equal(RecordSingleExceptionMiddleware.Message, ex.Message);
        Assert.Contains("ThrowOriginal", ex.StackTrace);
    }

    [Fact]
    public async Task ExecuteUnhandled_TwoAddExceptionCalls_ThrowsAggregateExceptionWithBothInOrder()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RecordTwoExceptionsMiddleware>());

        var ex = await Assert.ThrowsAsync<AggregateException>(() => sut.ExecuteUnhandled(new SingleHandler.Request(true)));

        Assert.Equal(new [] { RecordTwoExceptionsMiddleware.First, RecordTwoExceptionsMiddleware.Second }, ex.InnerExceptions);
    }

    [Fact]
    public async Task DispatchUnhandled_TwoAddExceptionCalls_ThrowsAggregateExceptionWithBothInOrder()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RecordTwoExceptionsMiddleware>());

        var ex = await Assert.ThrowsAsync<AggregateException>(() => sut.DispatchUnhandled(new SingleHandler.Message(true)));

        Assert.Equal(new [] { RecordTwoExceptionsMiddleware.First, RecordTwoExceptionsMiddleware.Second }, ex.InnerExceptions);
    }

    /// <summary>
    /// A middleware calls AddException, then lets the pipeline continue to a handler which throws its own,
    /// different exception directly (not via AddException). The directly-thrown exception must be what the
    /// caller observes (Dispatch/Execute never install a catch around ProcessPipeline in the *Unhandled variants,
    /// so this falls out of the pipeline structurally); the earlier AddException call must not be silently lost.
    /// </summary>
    [Fact]
    public async Task ExecuteUnhandled_HandlerThrowsAfterEarlierMiddlewareRecordedDifferentException_ThrownExceptionWinsAndRecordedOneStaysReadable()
    {
        var services = Factory.CreateServiceProvider((c, sc) =>
        {
            c.AddActionsFromAssembly(Factory.Assembly)
                .AddActionsFromAssemblyOf<SingleHandler.Message>()
                .AddHandlersFromAssembly(Factory.Assembly)
                .AddHandlersFromAssemblyOf<SingleHandler.MessageHandler>()
                .Use<RecordThenLetHandlerThrowMiddleware>(s => s.AddSingleton<RecordThenLetHandlerThrowMiddleware>(),
                    ServiceLifetime.Singleton);
        });
        var sut = services.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<SingleHandler.RequestException>(() => sut.ExecuteUnhandled(new SingleHandler.Request(false)));

        var middleware = services.GetRequiredService<RecordThenLetHandlerThrowMiddleware>();
        Assert.NotNull(middleware.CapturedContext);
        Assert.Same(RecordThenLetHandlerThrowMiddleware.EarlierException, Assert.Single(middleware.CapturedContext!.Exceptions));
    }

    /// <summary>
    /// Inner nested Dispatch (not Unhandled) records via AddException and fails; the outer handler inspects
    /// response.Success and recovers instead of rethrowing. Proves Exceptions never auto-propagates to the
    /// parent context the way Notifications do via NotificationPropagationMiddleware.
    /// </summary>
    [Fact]
    public async Task Dispatch_NestedInnerDispatchRecordsExceptionAndOuterRecovers_OuterOwnExceptionsStayEmpty()
    {
        OuterRecoveringActionHandler.CapturedOuterExceptionsAfterRecovery = null;
        var sut = Factory.CreateConfiguredMediator(c => c.UseWhenAction<InnerFailingAction, RecordExceptionAndFailMiddleware>());

        var result = await sut.Dispatch(new OuterRecoveringAction());

        Assert.True(result.Success);
        Assert.NotNull(OuterRecoveringActionHandler.CapturedOuterExceptionsAfterRecovery);
        Assert.Empty(OuterRecoveringActionHandler.CapturedOuterExceptionsAfterRecovery!);
    }

    /// <summary>
    /// The outer handler wraps a nested ExecuteUnhandled call in a plain try/catch. It must observe the original
    /// recorded exception type directly - proving the outer pipeline needs no special-casing for exceptions that
    /// bubble up through a nested *Unhandled call (forward-reference to Unit 4's boundary resolution, which relies
    /// on seeing the real business exception type here rather than a generic wrapper).
    /// </summary>
    [Fact]
    public async Task Dispatch_NestedExecuteUnhandledException_CaughtByOrdinaryOuterTryCatchAsOriginalType()
    {
        OuterCatchingActionHandler.Caught = null;
        var sut = Factory.CreateConfiguredMediator(c => c.UseWhenAction<SingleHandler.Request, RecordSingleExceptionMiddleware>());

        var result = await sut.Dispatch(new OuterCatchingAction());

        Assert.True(result.Success);
        var caught = Assert.IsType<ValidationException>(OuterCatchingActionHandler.Caught);
        Assert.Equal(RecordSingleExceptionMiddleware.Message, caught.Message);
    }

    private class ValidationException(string message) : Exception(message);

    private class RecordSingleExceptionMiddleware : IMediatorMiddleware
    {
        public const string Message = "validation failed";

        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            try
            {
                ThrowOriginal();
            }
            catch (Exception e)
            {
                context.AddException(e);
            }

            // Do not call next - mirrors a terminal validator middleware that stops the pipeline (like
            // E2E.NoHandlerWithoutErrorTests.BlockRequestMiddleware), so the handler never runs.
            return Task.CompletedTask;
        }

        private static void ThrowOriginal()
        {
            throw new ValidationException(Message);
        }
    }

    private class RecordTwoExceptionsMiddleware : IMediatorMiddleware
    {
        public static readonly Exception First = new InvalidOperationException("first");
        public static readonly Exception Second = new ArgumentException("second");

        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddException(First);
            context.AddException(Second);
            return Task.CompletedTask;
        }
    }

    private class RecordThenLetHandlerThrowMiddleware : IMediatorMiddleware
    {
        public static readonly Exception EarlierException = new InvalidOperationException("earlier recorded");

        public MediatorContext? CapturedContext { get; private set; }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            CapturedContext = context;
            context.AddException(EarlierException);
            await next(context);
        }
    }

    private record OuterRecoveringAction : IMediatorAction;

    private record OuterRecoveringActionHandler(IMediatorFacade Facade) : IMediatorHandler<OuterRecoveringAction>
    {
        public static IReadOnlyCollection<Exception>? CapturedOuterExceptionsAfterRecovery;

        public async Task Handle(OuterRecoveringAction action, CancellationToken cancellationToken)
        {
            var outerContext = Facade.Context;
            await Facade.Dispatch(new InnerFailingAction(), cancellationToken);
            // Deliberately does not inspect/rethrow - simulates a handler that recovers from the inner failure.
            CapturedOuterExceptionsAfterRecovery = outerContext!.Exceptions;
        }
    }

    private record InnerFailingAction : IMediatorAction;

    private class RecordExceptionAndFailMiddleware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddException(new InvalidOperationException("inner failure"));
            // Do not call next - the inner handler never runs.
            return Task.CompletedTask;
        }
    }

    private record OuterCatchingAction : IMediatorAction;

    private record OuterCatchingActionHandler(IMediatorFacade Facade) : IMediatorHandler<OuterCatchingAction>
    {
        public static Exception? Caught;

        public async Task Handle(OuterCatchingAction action, CancellationToken cancellationToken)
        {
            try
            {
                await Facade.ExecuteUnhandled(new SingleHandler.Request(true), cancellationToken);
            }
            catch (Exception e)
            {
                Caught = e;
            }
        }
    }
}
