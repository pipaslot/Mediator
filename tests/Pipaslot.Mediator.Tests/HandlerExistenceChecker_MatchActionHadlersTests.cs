using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pipaslot.Mediator.Tests;

public class HandlerExistenceChecker_MatchActionHadlersTests
{
    [Test]
    [Arguments(typeof(Message), new[] { typeof(ConcurrentMessageHandler), typeof(SequenceMessageHandler) })]
    [Arguments(typeof(Message), new[] { typeof(ConcurrentMessageHandler), typeof(SingleMessageHandler) })]
    [Arguments(typeof(Message), new[] { typeof(SequenceMessageHandler), typeof(SingleMessageHandler) })]
    [Arguments(typeof(Request), new[] { typeof(ConcurrentRequestHandler), typeof(SequenceRequestHandler) })]
    [Arguments(typeof(Request), new[] { typeof(ConcurrentRequestHandler), typeof(SingleRequestHandler) })]
    [Arguments(typeof(Request), new[] { typeof(SequenceRequestHandler), typeof(SingleRequestHandler) })]
    public async Task TestCombineInvalid(Type subject, Type[] handlers)
    {
        var sut = CreateServiceProviderWithHandlersAndActions(handlers, subject);
        var exception = await Assert.That(() =>
        {
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        }).Throws<MediatorException>();
        await CompareExceptions(MediatorException.CreateForCanNotCombineHandlers(subject, handlers), exception);
    }

    [Test]
    [Arguments(typeof(Message), new[] { typeof(SingleMessageHandler) })]
    [Arguments(typeof(Message), new[] { typeof(ConcurrentMessageHandler), typeof(ConcurrentMessage2Handler) })]
    [Arguments(typeof(Message), new[] { typeof(SequenceMessageHandler), typeof(SequenceMessage2Handler) })]
    [Arguments(typeof(Request), new[] { typeof(SingleRequestHandler) })]
    [Arguments(typeof(Request), new[] { typeof(ConcurrentRequestHandler), typeof(ConcurrentRequest2Handler) })]
    [Arguments(typeof(Request), new[] { typeof(SequenceRequestHandler), typeof(SequenceRequest2Handler) })]
    public async Task TestCombineValid(Type subject, Type[] handlers)
    {
        var sut = CreateServiceProviderWithHandlersAndActions(handlers, subject);
        await Assert.That(() =>
        {
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        }).DoesNotThrow();
    }

    [Test]
    [Arguments(typeof(Message))]
    [Arguments(typeof(Request))]
    public async Task TestNoHandler(Type subject)
    {
        var sut = CreateServiceProviderWithHandlersAndActions([], subject);
        var exception = await Assert.That(() =>
        {
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        }).Throws<MediatorException>();
        await CompareExceptions(MediatorExecutionException.CreateForNoHandler(subject), exception);
    }

    [Test]
    [Arguments(typeof(Message), new[] { typeof(SingleMessageHandler), typeof(SingleMessage2Handler) })]
    [Arguments(typeof(Request), new[] { typeof(SingleRequestHandler), typeof(SingleRequest2Handler) })]
    public async Task TestSingleInvalid(Type subject, Type[] handlers)
    {
        var sut = CreateServiceProviderWithHandlersAndActions(handlers, subject);
        var exception = await Assert.That(() =>
        {
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        }).Throws<MediatorException>();
        await CompareExceptions(MediatorException.CreateForDuplicateHandlers(subject, handlers), exception);
    }

    private IHandlerExistenceChecker CreateServiceProviderWithHandlersAndActions(Type[] handlers, Type subject)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddMediator();
        collection.RegisterHandlers(new Dictionary<Type, ServiceLifetime>(), handlers);
        collection.AddSingleton<IActionTypeProvider>(new ReflectionCache().AddActions(subject));
        var sp = collection.BuildServiceProvider();
        return sp.GetRequiredService<IHandlerExistenceChecker>();
    }

    private async Task CompareExceptions(MediatorException expected, MediatorException actual)
    {
        var data = actual.Data.GetEnumerator();
        data.MoveNext();
        var msg = (string)(data.Value ?? string.Empty);
        await Assert.That(msg).IsEqualTo(expected.Message);
    }

    #region Actions

    private class Request : IRequest<Response>;

    private class Response;

    private class Message : IMessage;

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