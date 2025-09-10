using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;

namespace Pipaslot.Mediator.Tests;

public class Mediator_CancelaltionTests
{
    #region Execute single handler

    [Test]
    public async Task Execute_TaskCancelled_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        var result = await sut.Execute(new SingleHandler.Request(true));
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.GetErrorMessage()).IsEqualTo(new TaskCanceledException().Message);
    }

    [Test]
    public async Task Execute_OperationCancelled_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        var result = await sut.Execute(new SingleHandler.Request(true));
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.GetErrorMessage()).IsEqualTo(new OperationCanceledException().Message);
    }

    #endregion

    #region ExecuteUnhandled single handler

    [Test]
    public async Task ExecuteUnhandled_TaskCancelled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        await Assert.That(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(true));
        }).Throws<TaskCanceledException>();
    }

    [Test]
    public async Task ExecuteUnhandled_OperationCancelled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        await Assert.That(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(true));
        }).Throws<OperationCanceledException>();
    }

    #endregion

    #region Dispatch single handler

    [Test]
    public async Task Dispatch_TaskCancelled_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.GetErrorMessage()).IsEqualTo(new TaskCanceledException().Message);
    }

    [Test]
    public async Task Dispatch_OperationCancelled_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.GetErrorMessage()).IsEqualTo(new OperationCanceledException().Message);
    }

    #endregion

    #region DispatchUnhandled single handler

    [Test]
    public async Task DispatchUnhandled_TaskCancelled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<TaskCancelledMediatorException>());
        await Assert.That(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(true));
        }).Throws<TaskCanceledException>();
    }

    [Test]
    public async Task DispatchUnhandled_OperationCancelled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator(s => s.Use<OperationCancelledMediatorException>());
        await Assert.That(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(true));
        }).Throws<OperationCanceledException>();
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