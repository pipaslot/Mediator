using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ErrorHandling;

public class CancellationTests
{
    #region Execute single handler

    [Fact]
    public async Task Execute_TaskCancelled_FailureWithGenericErrorDueToMissingExceptionHandler()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_OperationCancelled_FailureWithGenericErrorDueToMissingExceptionHandler()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    #endregion

    #region ExecuteUnhandled single handler

    [Fact]
    public async Task ExecuteUnhandled_TaskCancelled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(true));
        });
    }

    [Fact]
    public async Task ExecuteUnhandled_OperationCancelled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(true));
        });
    }

    #endregion

    #region Dispatch single handler

    [Fact]
    public async Task Dispatch_TaskCancelled_FailureWithGenericErrorDueToMissingExceptionHandler()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Dispatch_OperationCancelled_FailureWithGenericErrorDueToMissingExceptionHandler()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    #endregion

    #region DispatchUnhandled single handler

    [Fact]
    public async Task DispatchUnhandled_TaskCancelled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(true));
        });
    }

    [Fact]
    public async Task DispatchUnhandled_OperationCancelled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(true));
        });
    }

    #endregion

    #region OperationCanceledExceptionHandler (opt-in)

    /// <summary>
    /// The shipped <see cref="OperationCanceledExceptionHandler"/> is registered like any other typed handler. Registered
    /// for <see cref="OperationCanceledException"/>, it also covers <see cref="TaskCanceledException"/> through the
    /// resolver's subtype fallback - which is why the TaskCanceled variants below need no separate registration.
    /// </summary>
    [Fact]
    public async Task Execute_TaskCancelledWithRegisteredHandler_ReturnsHandlerMessageAndLogsWarning()
    {
        var (sut, logger) = Factory.CreateConfiguredMediatorWithLogger(c =>
        {
            c.Use<TaskCancelledMediatorException>();
            c.AddExceptionHandler<OperationCanceledExceptionHandler>();
        });

        var result = await sut.Execute(new SingleHandler.Request(true));

        Assert.False(result.Success);
        Assert.Equal(OperationCanceledExceptionHandler.Message, result.GetErrorMessage());
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Execute_OperationCancelledWithRegisteredHandler_ReturnsHandlerMessage()
    {
        var sut = Factory.CreateConfiguredMediator(c =>
        {
            c.Use<OperationCancelledMediatorException>();
            c.AddExceptionHandler<OperationCanceledExceptionHandler>();
        });

        var result = await sut.Execute(new SingleHandler.Request(true));

        Assert.False(result.Success);
        Assert.Equal(OperationCanceledExceptionHandler.Message, result.GetErrorMessage());
    }

    [Fact]
    public async Task Dispatch_TaskCancelledWithRegisteredHandler_ReturnsHandlerMessage()
    {
        var sut = Factory.CreateConfiguredMediator(c =>
        {
            c.Use<TaskCancelledMediatorException>();
            c.AddExceptionHandler<OperationCanceledExceptionHandler>();
        });

        var result = await sut.Dispatch(new SingleHandler.Message(true));

        Assert.False(result.Success);
        Assert.Equal(OperationCanceledExceptionHandler.Message, result.GetErrorMessage());
    }

    /// <summary>
    /// Translation is a concern of the catching boundary only - registering the handler must not change what the
    /// *Unhandled family throws, which stays the original cancellation exception.
    /// </summary>
    [Fact]
    public async Task ExecuteUnhandled_TaskCancelledWithRegisteredHandler_StillThrowsOriginalException()
    {
        var sut = Factory.CreateConfiguredMediator(c =>
        {
            c.Use<TaskCancelledMediatorException>();
            c.AddExceptionHandler<OperationCanceledExceptionHandler>();
        });

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(true));
        });
    }

    [Fact]
    public async Task DispatchUnhandled_OperationCancelledWithRegisteredHandler_StillThrowsOriginalException()
    {
        var sut = Factory.CreateConfiguredMediator(c =>
        {
            c.Use<OperationCancelledMediatorException>();
            c.AddExceptionHandler<OperationCanceledExceptionHandler>();
        });

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(true));
        });
    }

    #endregion

    public class TaskCancelledMediatorException : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            throw new TaskCanceledException();
        }
    }

    public class OperationCancelledMediatorException : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            throw new OperationCanceledException();
        }
    }
}