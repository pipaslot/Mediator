using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class HandlerExistenceChecker_MatchActionHadlersTests
    {

        [Theory]
        [InlineData(typeof(Message), new[] { typeof(ConcurrentMessageHandler), typeof(SequenceMessageHandler) })]
        [InlineData(typeof(Message), new[] { typeof(ConcurrentMessageHandler), typeof(SingleMessageHandler) })]
        [InlineData(typeof(Message), new[] { typeof(SequenceMessageHandler), typeof(SingleMessageHandler) })]
        [InlineData(typeof(Request), new[] { typeof(ConcurrentRequestHandler), typeof(SequenceRequestHandler) })]
        [InlineData(typeof(Request), new[] { typeof(ConcurrentRequestHandler), typeof(SingleRequestHandler) })]
        [InlineData(typeof(Request), new[] { typeof(SequenceRequestHandler), typeof(SingleRequestHandler) })]
        public void TestCombineInvalid(Type subject, Type[] handlers)
        {
            var sut = CreateServiceProviderWithHandlersAndActions(handlers, subject);
            var exception = Assert.Throws<MediatorException>(() =>
            {
                sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
            });
            CompareExceptions(MediatorException.CreateForCanNotCombineHandlers(subject, handlers), exception);
        }

        [Theory]
        [InlineData(typeof(Message), new[] { typeof(SingleMessageHandler) })]
        [InlineData(typeof(Message), new[] { typeof(ConcurrentMessageHandler), typeof(ConcurrentMessage2Handler) })]
        [InlineData(typeof(Message), new[] { typeof(SequenceMessageHandler), typeof(SequenceMessage2Handler) })]
        [InlineData(typeof(Request), new[] { typeof(SingleRequestHandler) })]
        [InlineData(typeof(Request), new[] { typeof(ConcurrentRequestHandler), typeof(ConcurrentRequest2Handler) })]
        [InlineData(typeof(Request), new[] { typeof(SequenceRequestHandler), typeof(SequenceRequest2Handler) })]
        public void TestCombineValid(Type subject, Type[] handlers)
        {
            var sut = CreateServiceProviderWithHandlersAndActions(handlers, subject);
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        }

        [Theory]
        [InlineData(typeof(Message))]
        [InlineData(typeof(Request))]
        public void TestNoHandler(Type subject)
        {
            var sut = CreateServiceProviderWithHandlersAndActions(new Type[0], subject);
            var exception = Assert.Throws<MediatorException>(() =>
            {
                sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
            });
            CompareExceptions(MediatorExecutionException.CreateForNoHandler(subject), exception);
        }

        [Theory]
        [InlineData(typeof(Message), new[] { typeof(SingleMessageHandler), typeof(SingleMessage2Handler) })]
        [InlineData(typeof(Request), new[] { typeof(SingleRequestHandler), typeof(SingleRequest2Handler) })]
        public void TestSingleInvalid(Type subject, Type[] handlers)
        {
            var sut = CreateServiceProviderWithHandlersAndActions(handlers, subject);
            var exception = Assert.Throws<MediatorException>(() =>
            {
                sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
            });
            CompareExceptions(MediatorException.CreateForDuplicateHandlers(subject, handlers), exception);
        }

        private IHandlerExistenceChecker CreateServiceProviderWithHandlersAndActions(Type[] handlers, Type subject)
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddMediator();
            collection.RegisterHandlers(handlers);
            collection.AddSingleton<IActionTypeProvider>(new FakeActionTypeProvider(subject));
            var sp = collection.BuildServiceProvider();
            return sp.GetService<IHandlerExistenceChecker>();
        }

        private void CompareExceptions(MediatorException expected, MediatorException actual)
        {
            var data = actual.Data.GetEnumerator();
            data.MoveNext();
            var msg = (string)data.Value;
            Assert.Equal(expected.Message, msg);
        }

        private class FakeActionTypeProvider : IActionTypeProvider
        {
            private Type[] _types;

            public FakeActionTypeProvider(params Type[] types)
            {
                _types = types;
            }

            public ICollection<Type> GetActionTypes()
            {
                return _types;
            }

            public ICollection<Type> GetMessageActionTypes()
            {
                return MediatorConfigurator.FilterAssignableToMessage(_types);
            }

            public ICollection<Type> GetRequestActionTypes()
            {
                return MediatorConfigurator.FilterAssignableToRequest(_types);
            }
        }

        #region Actions

        private class Request : IRequest<Response>
        {
        }

        private class Response
        {
        }

        private class Message : IMessage
        {
        }

        private class SingleRequestHandler : IRequestHandler<Request, Response>
        {
            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
        private class SingleRequest2Handler : IRequestHandler<Request, Response>
        {
            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class ConcurrentRequestHandler : IRequestHandler<Request, Response>, IConcurrentHandler
        {
            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class ConcurrentRequest2Handler : IRequestHandler<Request, Response>, IConcurrentHandler
        {
            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class SequenceRequestHandler : IRequestHandler<Request, Response>, ISequenceHandler
        {
            public int Order => 1;

            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class SequenceRequest2Handler : IRequestHandler<Request, Response>, ISequenceHandler
        {
            public int Order => 1;

            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }


        private class SingleMessageHandler : IMessageHandler<Message>
        {
            public Task Handle(Message Message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
        private class SingleMessage2Handler : IMessageHandler<Message>
        {
            public Task Handle(Message Message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class ConcurrentMessageHandler : IMessageHandler<Message>, IConcurrentHandler
        {
            public Task Handle(Message Message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class ConcurrentMessage2Handler : IMessageHandler<Message>, IConcurrentHandler
        {
            public Task Handle(Message Message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class SequenceMessageHandler : IMessageHandler<Message>, ISequenceHandler
        {
            public int Order => 1;

            public Task Handle(Message Message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private class SequenceMessage2Handler : IMessageHandler<Message>, ISequenceHandler
        {
            public int Order => 1;

            public Task Handle(Message Message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
