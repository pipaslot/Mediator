﻿using Pipaslot.Mediator.Server;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests.Server
{
    public class Mediator_MediatorExceptionLoggingMiddlewareTests
    {
        [Fact]
        public async Task Execute_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsSuccessFalse()
        {
            var sut = CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Execute_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsErrorMessage()
        {
            var sut = CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.Equal(SingleHandler.RequestException.Message, result.ErrorMessage);
        }

        [Fact]
        public async Task ExecuteUnhandled_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsException()
        {
            var sut = CreateMediator();
            await Assert.ThrowsAsync<SingleHandler.RequestException>(async () =>
            {
                await sut.ExecuteUnhandled(new SingleHandler.Request(false));
            });
        }

        [Fact]
        public async Task Dispatch_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsSuccessFalse()
        {
            var sut = CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Dispatch_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsErrorMessage()
        {
            var sut = CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(false));
            Assert.Equal(SingleHandler.MessageException.Message, result.ErrorMessage);
        }

        [Fact]
        public async Task DispatchUnhandled_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsException()
        {
            var sut = CreateMediator();
            await Assert.ThrowsAsync<SingleHandler.MessageException>(async () =>
            {
                await sut.DispatchUnhandled(new SingleHandler.Message(false));
            });
        }

        private IMediator CreateMediator()
        {
            return Factory.CreateMediator(c => c.AddDefaultPipeline().UseExceptionLogging());
        }
    }
}