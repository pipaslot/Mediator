using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class Mediator_SingleHandlerTests
    {
        #region Execute single handler

        [Fact]
        public async Task Execute_PassingHandler_SuccessAsTrue()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(true));
            Assert.True(result.Success);
        }

        [Fact]
        public async Task Execute_PassingHandler_EmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(true));
            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        [Fact]
        public async Task Execute_PassingHandler_ResultReturnsDataFromHandler()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(true));
            Assert.Equal(SingleHandler.Response.Instance, result.Result);
        }

        [Fact]
        public async Task Execute_FailingHandler_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Execute_FailingHandler_NotEmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.Equal(SingleHandler.RequestException.Message, result.ErrorMessage);
        }

        [Fact]
        public async Task Execute_FailingHandler_NullResult()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.Null(result.Result);
        }

        #endregion

        #region ExecuteUnhandled single handler

        [Fact]
        public async Task ExecuteUnhandled_PassingHandler_ReturnsResult()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.ExecuteUnhandled(new SingleHandler.Request(true));
            Assert.Equal(SingleHandler.Response.Instance, result);
        }

        [Fact]
        public async Task ExecuteUnhandled_FailingHandler_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            await Assert.ThrowsAsync<SingleHandler.RequestException>(async () =>
            {
                await sut.ExecuteUnhandled(new SingleHandler.Request(false));
            });
        }

        #endregion

        #region Dispatch single handler

        [Fact]
        public async Task Dispatch_PassingHandler_SuccessAsTrue()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(true));
            Assert.True(result.Success);
        }

        [Fact]
        public async Task Dispatch_PassingHandler_EmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(true));
            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        [Fact]
        public async Task Dispatch_FailingHandler_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Dispatch_FailingHandler_NotEmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(false));
            Assert.Equal(SingleHandler.MessageException.Message, result.ErrorMessage);
        }

        #endregion

        #region DispatchUnhandled single handler

        [Fact]
        public async Task DispatchUnhandled_PassingHandler_NoAction()
        {
            var sut = Factory.CreateMediator();
            await sut.DispatchUnhandled(new SingleHandler.Message(true));
        }

        [Fact]
        public async Task DispatchUnhandled_FailingHandler_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            await Assert.ThrowsAsync<SingleHandler.MessageException>(async () =>
            {
                await sut.DispatchUnhandled(new SingleHandler.Message(false));
            });
        }

        #endregion

        #region Actions and handlers
        public static class SingleHandler
        {
            public class Request : IRequest<Response>
            {
                public bool Pass { get; }

                public Request(bool pass)
                {
                    Pass = pass;
                }
            }
            public class Response
            {
                public static Response Instance = new Response();
            }

            public class Message : IMessage
            {
                public bool Pass { get; }

                public Message(bool pass)
                {
                    Pass = pass;
                }
            }

            public class RequestException : System.Exception
            {
                public static string Message = "REquesthandler failed";

                public RequestException() : base(Message)
                {
                }
            }

            public class MessageException : System.Exception
            {
                public static string Message = "Message handler failed";

                public MessageException() : base(Message)
                {
                }
            }

            public class RequestHandler : IRequestHandler<Request, Response>
            {
                public Task<Response> Handle(Request request, CancellationToken cancellationToken)
                {
                    if (!request.Pass)
                    {
                        throw new RequestException();
                    }
                    return Task.FromResult(Response.Instance);
                }
            }

            public class MessageHandler : IMessageHandler<SingleHandler.Message>
            {
                public Task Handle(Message request, CancellationToken cancellationToken)
                {
                    if (!request.Pass)
                    {
                        throw new MessageException();
                    }
                    return Task.CompletedTask;
                }
            }
        }

        #endregion
    }
}
