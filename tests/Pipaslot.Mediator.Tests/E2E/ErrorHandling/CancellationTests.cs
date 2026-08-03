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
        var sut = CreateMediator<TaskCancelledMediatorException>(typeof(SingleHandler.RequestHandler));
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_OperationCancelled_FailureWithGenericErrorDueToMissingExceptionHandler()
    {
        var sut = CreateMediator<OperationCancelledMediatorException>(typeof(SingleHandler.RequestHandler));
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    #endregion

    #region ExecuteUnhandled single handler

    [Fact]
    public async Task ExecuteUnhandled_TaskCancelled_ReturnsResult()
    {
        var sut = CreateMediator<TaskCancelledMediatorException>(typeof(SingleHandler.RequestHandler));
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(true));
        });
    }

    [Fact]
    public async Task ExecuteUnhandled_OperationCancelled_ReturnsResult()
    {
        var sut = CreateMediator<OperationCancelledMediatorException>(typeof(SingleHandler.RequestHandler));
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
        var sut = CreateMediator<TaskCancelledMediatorException>(typeof(SingleHandler.MessageHandler));
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Dispatch_OperationCancelled_FailureWithGenericErrorDueToMissingExceptionHandler()
    {
        var sut = CreateMediator<OperationCancelledMediatorException>(typeof(SingleHandler.MessageHandler));
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    #endregion

    #region DispatchUnhandled single handler

    [Fact]
    public async Task DispatchUnhandled_TaskCancelled_ReturnsResult()
    {
        var sut = CreateMediator<TaskCancelledMediatorException>(typeof(SingleHandler.MessageHandler));
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(true));
        });
    }

    [Fact]
    public async Task DispatchUnhandled_OperationCancelled_ReturnsResult()
    {
        var sut = CreateMediator<OperationCancelledMediatorException>(typeof(SingleHandler.MessageHandler));
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
        var (sut, logger) = CreateMediatorWithLogger<TaskCancelledMediatorException>(typeof(SingleHandler.RequestHandler));

        var result = await sut.Execute(new SingleHandler.Request(true));

        Assert.False(result.Success);
        Assert.Equal(OperationCanceledExceptionHandler.Message, result.GetErrorMessage());
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Execute_OperationCancelledWithRegisteredHandler_ReturnsHandlerMessage()
    {
        var sut = CreateMediatorWithExceptionHandler<OperationCancelledMediatorException>(typeof(SingleHandler.RequestHandler));

        var result = await sut.Execute(new SingleHandler.Request(true));

        Assert.False(result.Success);
        Assert.Equal(OperationCanceledExceptionHandler.Message, result.GetErrorMessage());
    }

    [Fact]
    public async Task Dispatch_TaskCancelledWithRegisteredHandler_ReturnsHandlerMessage()
    {
        var sut = CreateMediatorWithExceptionHandler<TaskCancelledMediatorException>(typeof(SingleHandler.MessageHandler));

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
        var sut = CreateMediatorWithExceptionHandler<TaskCancelledMediatorException>(typeof(SingleHandler.RequestHandler));

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(true));
        });
    }

    [Fact]
    public async Task DispatchUnhandled_OperationCancelledWithRegisteredHandler_StillThrowsOriginalException()
    {
        var sut = CreateMediatorWithExceptionHandler<OperationCancelledMediatorException>(typeof(SingleHandler.MessageHandler));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(true));
        });
    }

    #endregion

    private static IMediator CreateMediator<TCancellationMiddleware>(Type handlerType)
        where TCancellationMiddleware : IMediatorMiddleware
    {
        return Factory.CreateCustomMediator(c => c
            .AddHandlers([handlerType])
            .Use<TCancellationMiddleware>());
    }

    private static IMediator CreateMediatorWithExceptionHandler<TCancellationMiddleware>(Type handlerType)
        where TCancellationMiddleware : IMediatorMiddleware
    {
        return Factory.CreateCustomMediator(c =>
        {
            c.AddHandlers([handlerType]);
            c.Use<TCancellationMiddleware>();
            c.AddExceptionHandler<OperationCanceledExceptionHandler>();
        });
    }

    private static (IMediator Mediator, TestLogger<Mediator> Logger) CreateMediatorWithLogger<TCancellationMiddleware>(Type handlerType)
        where TCancellationMiddleware : IMediatorMiddleware
    {
        return Factory.CreateConfiguredMediatorWithLogger(c =>
        {
            c.AddHandlers([handlerType]);
            c.Use<TCancellationMiddleware>();
            c.AddExceptionHandler<OperationCanceledExceptionHandler>();
        });
    }

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