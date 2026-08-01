using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E;

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
public class HandlerThrowsWithRegisteredExceptionHandler
{
    [Fact]
    public async Task Execute_RegisteredHandlerMatchesThrownType_ResultsContainTranslatedMessageOnly()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.AddExceptionHandler<RequestExceptionHandler>());

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(RequestExceptionHandler.TranslatedMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_RegisteredHandlerMatchesThrownType_LogsWarningNotError()
    {
        var (sut, logger) = Factory.CreateConfiguredMediatorWithLogger(c => c.AddExceptionHandler<RequestExceptionHandler>());

        await sut.Execute(new SingleHandler.Request(false));

        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Warning);
        Assert.IsType<SingleHandler.RequestException>(entry.Exception);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);
    }

    /// <summary>
    /// A typed exception handler that itself throws while translating must not crash the call or mask the
    /// original exception - the boundary falls through to the same safe-by-default fallback as an unmapped
    /// exception. Exercises that guarantee through the real Execute/Dispatch boundary, not the standalone
    /// resolver/executor.
    /// </summary>
    [Fact]
    public async Task Execute_RegisteredHandlerItselfThrows_DegradesToGenericMessageAndErrorLog()
    {
        var (sut, logger) = Factory.CreateConfiguredMediatorWithLogger(c => c.AddExceptionHandler<ThrowingRequestExceptionHandler>());

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.IsType<SingleHandler.RequestException>(entry.Exception);
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
        var sut = Factory.CreateConfiguredMediator(c => c.UseWhenAction<SingleHandler.Request, RecordThenContinueMiddleware>());

        var result = await sut.Execute(new SingleHandler.Request(false));

        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
        Assert.DoesNotContain(RecordThenContinueMiddleware.RecordedSecretMessage, result.GetErrorMessage());
    }

    private class RequestExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public const string TranslatedMessage = "The request could not be completed.";

        public Task<string> Handle(SingleHandler.RequestException exception, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(TranslatedMessage);
        }
    }

    private class ThrowingRequestExceptionHandler : IMediatorExceptionHandler<SingleHandler.RequestException>
    {
        public Task<string> Handle(SingleHandler.RequestException exception, System.Threading.CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Translator is broken.");
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
