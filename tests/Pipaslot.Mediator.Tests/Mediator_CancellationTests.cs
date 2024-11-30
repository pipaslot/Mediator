using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

public class Mediator_CancelaltionTests
{
    #region Execute single handler

    [Fact]
    public async Task Execute_TaskCancelled_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.False(result.Success);
        Assert.Equal(new TaskCanceledException().Message, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_OperationCancelled_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.False(result.Success);
        Assert.Equal(new OperationCanceledException().Message, result.GetErrorMessage());
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
    public async Task Dispatch_TaskCancelled_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        Assert.False(result.Success);
        Assert.Equal(new TaskCanceledException().Message, result.GetErrorMessage());
    }

    [Fact]
    public async Task Dispatch_OperationCancelled_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        Assert.False(result.Success);
        Assert.Equal(new OperationCanceledException().Message, result.GetErrorMessage());
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