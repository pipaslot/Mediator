using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ErrorHandling;

/// <summary>
/// Covers the typed-handler branch of the Execute/Dispatch boundary: an exception reaching the boundary for which
/// an <see cref="IMediatorExceptionHandler{TException}"/> is registered is translated instead of falling back to
/// the generic safe message, and the translation is logged at Warning (not Error) since it was handled on purpose.
/// Also covers the two failure-isolation guarantees the boundary must provide regardless of translation outcome:
/// a typed handler that itself throws must not crash the call, and <c>context.Exceptions</c> recorded by an
/// earlier middleware must never leak into <see cref="MediatorContext.Results"/> even when a later, unrelated
/// exception reaches the boundary unmapped. Exercises handler resolution wired into the real Execute/Dispatch
/// boundary, unlike <c>Middlewares/ExceptionHandlerResolutionTests.cs</c>, which only exercises the
/// resolver/executor standalone.
/// </summary>
public class HandlerThrowsWithRegisteredExceptionHandlerTests
{
    [Fact]
    public async Task Execute_RegisteredHandlerMatchesThrownType_ResultsContainTranslatedMessageOnly()
    {
        var sut = CreateMediator<RequestExceptionHandler>();

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(RequestExceptionHandler.TranslatedMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_RegisteredHandlerMatchesThrownType_LogsWarningNotError()
    {
        var (sut, logger) = CreateMediatorWithLogger<RequestExceptionHandler>();

        await sut.Execute(new SingleHandler.Request(false));

        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Warning);
        Assert.IsType<SingleHandler.RequestException>(entry.Exception);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);
    }

    /// <summary>
    /// A typed exception handler that itself throws while translating must not crash the call or mask the
    /// original exception - the boundary falls through to the same safe-by-default fallback as an unmapped
    /// exception. Exercises that guarantee through the real Execute/Dispatch boundary, not the standalone
    /// resolver/executor. Also proves the handler's own fault is no longer invisible: it gets its own Error entry,
    /// distinct from the original exception's own fallback entry, so "handler is broken" and "no handler registered"
    /// remain distinguishable from the log alone.
    /// </summary>
    [Fact]
    public async Task Execute_RegisteredHandlerItselfThrows_DegradesToGenericMessageAndErrorLog()
    {
        var (sut, logger) = CreateMediatorWithLogger<ThrowingRequestExceptionHandler>();

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
        var errorEntries = logger.Entries.Where(e => e.Level == LogLevel.Error).ToList();
        Assert.Equal(2, errorEntries.Count);
        Assert.Contains(errorEntries, e => e.Exception is SingleHandler.RequestException);
        Assert.Contains(errorEntries, e => e.Exception is InvalidOperationException);
    }

    /// <summary>
    /// A middleware records an exception via <c>AddException</c> and lets the pipeline continue; the handler then
    /// throws a different, unmapped exception directly. <c>context.Exceptions</c> is server-side-only by
    /// construction - this proves that guarantee still holds when the real Execute/Dispatch boundary is the one
    /// building <see cref="MediatorContext.Results"/>, not just inferred from the context API's structural
    /// separation.
    /// </summary>
    [Fact]
    public async Task Execute_EarlierRecordedExceptionNeverLeaksIntoResults()
    {
        var sut = Factory.CreateMediator(c => c
            .AddHandlers([typeof(SingleHandler.RequestHandler)])
            .UseWhenAction<SingleHandler.Request, RecordThenContinueMiddleware>());

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
        Assert.DoesNotContain(RecordThenContinueMiddleware.RecordedSecretMessage, result.GetErrorMessage());
    }

    /// <summary>
    /// Proves the pipeline context reaches the handler correctly, not just a translated message - the handler
    /// reports <see cref="MediatorContext.ActionIdentifier"/> back as its "translated" message.
    /// </summary>
    [Fact]
    public async Task Execute_RegisteredHandlerReadsActionIdentifierFromContext_ResultContainsIt()
    {
        var sut = CreateMediator<ActionIdentifierReportingExceptionHandler>();
        var action = new SingleHandler.Request(false);

        var result = await sut.Execute(action);

        Assert.False(result.Success);
        Assert.Equal(action.GetType().ToString(), result.GetErrorMessage());
    }

    /// <summary>
    /// <see cref="IMediatorExceptionContext.SetHandledWithoutMessage"/> still fails the action, but adds nothing to
    /// <see cref="MediatorContext.Results"/> - not even an empty-content notification.
    /// </summary>
    [Fact]
    public async Task Execute_RegisteredHandlerCallsSetHandledWithoutMessage_FailsWithEmptyResultsAndWarningLog()
    {
        var (sut, logger) = CreateMediatorWithLogger<HandledWithoutMessageExceptionHandler>();

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Empty(result.Results);
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning);
    }

    /// <summary>
    /// Proves the boundary does not also add a generic message when the handler already reported its own via
    /// <see cref="MediatorContext.AddError(string, bool)"/> before declining a client-facing message on the context itself.
    /// </summary>
    [Fact]
    public async Task Execute_RegisteredHandlerAddsOwnErrorThenCallsSetHandledWithoutMessage_ResultsContainOnlyItsOwnNotification()
    {
        var sut = CreateMediator<HandledWithoutMessageButOwnErrorExceptionHandler>();

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(HandledWithoutMessageButOwnErrorExceptionHandler.OwnMessage, Assert.Single(result.GetErrorMessages()));
    }

    /// <summary>
    /// <see cref="IMediatorExceptionContext.SetLogLevel"/> lets a handler escalate the boundary's own log entry for
    /// the original exception past the Warning default.
    /// </summary>
    [Fact]
    public async Task Execute_RegisteredHandlerSetsLogLevelToError_LogsSingleErrorEntryNoWarning()
    {
        var (sut, logger) = CreateMediatorWithLogger<ErrorLogLevelExceptionHandler>();

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.IsType<SingleHandler.RequestException>(entry.Exception);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Warning);
    }

    /// <summary>
    /// <see cref="LogLevel.None"/> suppresses the boundary's own log entry entirely - for a handler that already
    /// logs the failure itself - without affecting the translated message reported to the client.
    /// </summary>
    [Fact]
    public async Task Execute_RegisteredHandlerSetsLogLevelToNone_SuppressesLogEntryButKeepsTranslatedMessage()
    {
        var (sut, logger) = CreateMediatorWithLogger<SuppressedLogExceptionHandler>();

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(SuppressedLogExceptionHandler.TranslatedMessage, result.GetErrorMessage());
        Assert.Empty(logger.Entries);
    }

    /// <summary>
    /// A <see cref="IMediatorExceptionContext.SetLogLevel"/> call has no effect when the handler ends up not handling
    /// the exception - the unmapped-exception fallback owns its own Error entry regardless of what the declining
    /// handler did to the context first.
    /// </summary>
    [Fact]
    public async Task Execute_RegisteredHandlerSetsLogLevelButDoesNotCallSetHandled_FallsBackToGenericMessageAndErrorLog()
    {
        var (sut, logger) = CreateMediatorWithLogger<LogLevelSetButNotHandledExceptionHandler>();

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.IsType<SingleHandler.RequestException>(entry.Exception);
    }

    /// <summary>
    /// A handler registered for a base exception type inspects the concrete instance and declines to translate one
    /// subtype while translating another - the realistic case of a handler for a shared base type (e.g. a
    /// <c>SqlException</c>-style hierarchy) that only recognizes specific subtypes. Declining here is a plain return
    /// without calling <c>SetHandled*</c>, which is equivalent to <see cref="IMediatorExceptionContext.SetNotHandled"/>.
    /// </summary>
    [Fact]
    public async Task Execute_HandlerDeclinesOneSubtypeButTranslatesAnother_DeclinedGetsGenericTranslatedGetsItsMessage()
    {
        var (sut, logger) = Factory.CreateMediatorWithLogger(c =>
        {
            c.AddExceptionHandler<BaseExceptionHandler>();
            c.UseWhenAction<SingleHandler.Request, ThrowBaseOrDerivedExceptionMiddleware>();
        });

        var declinedResult = await sut.Execute(new SingleHandler.Request(false));
        var translatedResult = await sut.Execute(new SingleHandler.Request(true));

        Assert.False(declinedResult.Success);
        Assert.Equal(Mediator.GenericErrorMessage, declinedResult.GetErrorMessage());
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Exception is DeclinedSubException);

        Assert.False(translatedResult.Success);
        Assert.Equal(BaseExceptionHandler.TranslatedMessage, translatedResult.GetErrorMessage());
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning && e.Exception is TranslatedSubException);
    }

    /// <summary>
    /// <see cref="IMediatorExceptionContext.SetNotHandled"/> reverses an earlier <see cref="IMediatorExceptionContext.SetHandled(string)"/>
    /// call on the same context - the boundary falls back to its generic message, and the reversed message never
    /// reaches <see cref="MediatorContext.Results"/>.
    /// </summary>
    [Fact]
    public async Task Execute_RegisteredHandlerCallsSetHandledThenSetNotHandled_FallsBackToGenericMessageWithoutReversedMessage()
    {
        var sut = CreateMediator<HandledThenReversedExceptionHandler>();

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
        Assert.DoesNotContain(HandledThenReversedExceptionHandler.ReversedMessage, result.GetErrorMessage());
    }

    /// <summary>
    /// Resolution asks the single most specific registered handler once; a decline does not fall back to search for
    /// a less specific match. A base-type handler is registered alongside a derived-type handler that declines - the
    /// base handler's invocation counter proves it is never asked.
    ///
    /// This mirrors how a C# try/catch chain resolves: only the first, most specific matching catch runs, and if that
    /// block does not handle the exception (e.g. it rethrows), execution does not fall through to a broader catch
    /// further down in the same try - the exception simply propagates. For example:
    /// <code>
    /// try { ... }
    /// catch (SqlException ex) when (ex.Number == 1205) { /* only deadlocks */ }
    /// catch (Exception ex) { /* never reached for a SqlException with a different Number -
    ///                           the runtime does not "fall back" to this broader catch */ }
    /// </code>
    /// A less specific handler here is exactly that broader catch: declining in the more specific one must not
    /// silently route the exception to a handler that never expected to see this concrete type - it could translate
    /// it with a message that does not fit the case the specific handler explicitly rejected.
    /// </summary>
    [Fact]
    public async Task Execute_MostSpecificHandlerDeclines_DoesNotFallBackToLessSpecificHandler()
    {
        BaseCountingExceptionHandler.InvocationCount = 0;
        var sut = Factory.CreateMediator(c =>
        {
            c.AddExceptionHandler<BaseCountingExceptionHandler>();
            c.AddExceptionHandler<DerivedDecliningExceptionHandler>();
            c.UseWhenAction<SingleHandler.Request, ThrowDerivedCountingExceptionMiddleware>();
        });

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
        Assert.Equal(0, BaseCountingExceptionHandler.InvocationCount);
    }

    private static IMediator CreateMediator<TExceptionHandler>() where TExceptionHandler : class
    {
        return Factory.CreateMediator(c => c
            .AddHandlers([typeof(SingleHandler.RequestHandler)])
            .AddExceptionHandler<TExceptionHandler>());
    }

    private static (IMediator Mediator, TestLogger<Mediator> Logger) CreateMediatorWithLogger<TExceptionHandler>() where TExceptionHandler : class
    {
        return Factory.CreateMediatorWithLogger(c => c
            .AddHandlers([typeof(SingleHandler.RequestHandler)])
            .AddExceptionHandler<TExceptionHandler>());
    }

    private class RequestExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public const string TranslatedMessage = "The request could not be completed.";

        public Task Handle(SingleHandler.RequestException exception, IMediatorExceptionContext context)
        {
            context.SetHandled(TranslatedMessage);
            return Task.CompletedTask;
        }
    }

    private class ThrowingRequestExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public Task Handle(SingleHandler.RequestException exception, IMediatorExceptionContext context)
        {
            throw new InvalidOperationException("Translator is broken.");
        }
    }

    private class ActionIdentifierReportingExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public Task Handle(SingleHandler.RequestException exception, IMediatorExceptionContext context)
        {
            context.SetHandled(context.Context.ActionIdentifier);
            return Task.CompletedTask;
        }
    }

    private class HandledWithoutMessageExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public Task Handle(SingleHandler.RequestException exception, IMediatorExceptionContext context)
        {
            context.SetHandledWithoutMessage();
            return Task.CompletedTask;
        }
    }

    private class HandledWithoutMessageButOwnErrorExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public const string OwnMessage = "own message reported by the handler itself";

        public Task Handle(SingleHandler.RequestException exception, IMediatorExceptionContext context)
        {
            context.Context.AddError(OwnMessage);
            context.SetHandledWithoutMessage();
            return Task.CompletedTask;
        }
    }

    private class ErrorLogLevelExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public Task Handle(SingleHandler.RequestException exception, IMediatorExceptionContext context)
        {
            context.SetLogLevel(LogLevel.Error);
            context.SetHandled("Escalated translation.");
            return Task.CompletedTask;
        }
    }

    private class SuppressedLogExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public const string TranslatedMessage = "Suppressed-log translation.";

        public Task Handle(SingleHandler.RequestException exception, IMediatorExceptionContext context)
        {
            context.SetLogLevel(LogLevel.None);
            context.SetHandled(TranslatedMessage);
            return Task.CompletedTask;
        }
    }

    private class LogLevelSetButNotHandledExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public Task Handle(SingleHandler.RequestException exception, IMediatorExceptionContext context)
        {
            context.SetLogLevel(LogLevel.None);
            return Task.CompletedTask;
        }
    }

    private class BaseException(string message) : Exception(message);

    private class DeclinedSubException() : BaseException("declined subtype detail");

    private class TranslatedSubException() : BaseException("translated subtype detail");

    private class BaseExceptionHandler : IMediatorExceptionHandler<BaseException>
    {
        public const string TranslatedMessage = "A recoverable condition occurred.";

        public Task Handle(BaseException exception, IMediatorExceptionContext context)
        {
            if (exception is not DeclinedSubException)
            {
                context.SetHandled(TranslatedMessage);
            }

            return Task.CompletedTask;
        }
    }

    private class ThrowBaseOrDerivedExceptionMiddleware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var request = (SingleHandler.Request)context.Action;
            throw request.Pass ? new TranslatedSubException() : new DeclinedSubException();
        }
    }

    private class HandledThenReversedExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public const string ReversedMessage = "reversed-message-marker";

        public Task Handle(SingleHandler.RequestException exception, IMediatorExceptionContext context)
        {
            context.SetHandled(ReversedMessage);
            context.SetNotHandled();
            return Task.CompletedTask;
        }
    }

    private class BaseCountingException(string message) : Exception(message);

    private class DerivedDecliningException() : BaseCountingException("derived detail");

    private class BaseCountingExceptionHandler : IMediatorExceptionHandler<BaseCountingException>
    {
        public static int InvocationCount;

        public Task Handle(BaseCountingException exception, IMediatorExceptionContext context)
        {
            InvocationCount++;
            context.SetHandled("Base translation.");
            return Task.CompletedTask;
        }
    }

    private class DerivedDecliningExceptionHandler : IMediatorExceptionHandler<DerivedDecliningException>
    {
        public Task Handle(DerivedDecliningException exception, IMediatorExceptionContext context)
        {
            // Declines - the most specific match must not fall back to the less specific BaseCountingExceptionHandler.
            return Task.CompletedTask;
        }
    }

    private class ThrowDerivedCountingExceptionMiddleware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            throw new DerivedDecliningException();
        }
    }

    private class RecordThenContinueMiddleware : IMediatorMiddleware
    {
        public const string RecordedSecretMessage = "internal-only detail recorded via AddException";

        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddException(new InvalidOperationException(RecordedSecretMessage));
            return next(context);
        }
    }
}
