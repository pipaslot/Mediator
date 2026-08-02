using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// <see cref="MediatorExceptionContext"/> exercised standalone - no <see cref="IMediator"/> dispatch involved, mirrors
/// how <c>Middlewares/MediatorContextTests.cs</c> covers <see cref="Middlewares.MediatorContext"/>. Also the
/// regression guard for the mockability claim behind <see cref="IMediatorExceptionContext"/>: a handler must be
/// testable against a mock/fake of the interface with no DI container, real <c>MediatorContext</c> or mediator
/// anywhere in the test.
/// </summary>
public class MediatorExceptionContextTests
{
    [Fact]
    public void IsHandled_FreshContext_IsFalse()
    {
        var sut = new MediatorExceptionContext(new InvalidOperationException("boom"), Factory.FakeContext(new SingleHandler.Message(true)));

        Assert.False(sut.IsHandled);
    }

    [Fact]
    public void Message_FreshContext_IsNull()
    {
        var sut = new MediatorExceptionContext(new InvalidOperationException("boom"), Factory.FakeContext(new SingleHandler.Message(true)));

        Assert.Null(sut.Message);
    }

    [Fact]
    public void SetHandled_SetsIsHandledAndMessage()
    {
        var sut = new MediatorExceptionContext(new InvalidOperationException("boom"), Factory.FakeContext(new SingleHandler.Message(true)));

        sut.SetHandled("translated message");

        Assert.True(sut.IsHandled);
        Assert.Equal("translated message", sut.Message);
    }

    [Fact]
    public void SetHandledWithoutMessage_SetsIsHandledTrueAndMessageStaysNull()
    {
        var sut = new MediatorExceptionContext(new InvalidOperationException("boom"), Factory.FakeContext(new SingleHandler.Message(true)));

        sut.SetHandledWithoutMessage();

        Assert.True(sut.IsHandled);
        Assert.Null(sut.Message);
    }

    [Fact]
    public void LogLevel_FreshContext_DefaultsToWarning()
    {
        var sut = new MediatorExceptionContext(new InvalidOperationException("boom"), Factory.FakeContext(new SingleHandler.Message(true)));

        Assert.Equal(LogLevel.Warning, sut.LogLevel);
    }

    [Fact]
    public void SetLogLevel_ChangesLogLevel()
    {
        var sut = new MediatorExceptionContext(new InvalidOperationException("boom"), Factory.FakeContext(new SingleHandler.Message(true)));

        sut.SetLogLevel(LogLevel.None);

        Assert.Equal(LogLevel.None, sut.LogLevel);
    }

    [Fact]
    public void Exception_ExposesTheInstancePassedToConstructor()
    {
        var exception = new InvalidOperationException("boom");

        var sut = new MediatorExceptionContext(exception, Factory.FakeContext(new SingleHandler.Message(true)));

        Assert.Same(exception, sut.Exception);
    }

    [Fact]
    public void Context_ExposesTheInstancePassedToConstructor()
    {
        var context = Factory.FakeContext(new SingleHandler.Message(true));

        var sut = new MediatorExceptionContext(new InvalidOperationException("boom"), context);

        Assert.Same(context, sut.Context);
    }

    [Fact]
    public void CancellationToken_DefaultsToTheContextsToken()
    {
        var context = Factory.FakeContext(new SingleHandler.Message(true));

        var sut = new MediatorExceptionContext(new InvalidOperationException("boom"), context);

        Assert.Equal(context.CancellationToken, sut.CancellationToken);
    }

    [Fact]
    public void CancellationToken_ReflectsTokenReplacedOnContextViaSetCancellationToken()
    {
        var context = Factory.FakeContext(new SingleHandler.Message(true));
        using var cts = new CancellationTokenSource();
        context.SetCancellationToken(cts.Token);

        var sut = new MediatorExceptionContext(new InvalidOperationException("boom"), context);

        Assert.Equal(cts.Token, sut.CancellationToken);
    }

    /// <summary>
    /// Proves a handler can be unit-tested against a mocked <see cref="IMediatorExceptionContext"/> with no DI
    /// container, real <see cref="Middlewares.MediatorContext"/> or <see cref="IMediator"/> anywhere in the test. If
    /// this needs any of those to compile or run, the interface has failed the purpose it exists for.
    /// </summary>
    [Fact]
    public async Task OperationCanceledExceptionHandler_Handle_CallsSetHandledOnMockedContext()
    {
        var sut = new OperationCanceledExceptionHandler();
        var exception = new OperationCanceledException("cancelled");
        var contextMock = new Mock<IMediatorExceptionContext>();

        await sut.Handle(exception, contextMock.Object);

        contextMock.Verify(c => c.SetHandled(OperationCanceledExceptionHandler.Message), Times.Once);
    }
}
