using Pipaslot.Mediator.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Middlewares;

public class DirectCallProtectionMiddlewareTests
{
    [Fact]
    public async Task IndirectCall_ShouldPass()
    {
        var sut = CreateMediator();
        var res = await sut.Dispatch(new RootAction());
        Assert.True(res.Success);
    }

    [Fact]
    public async Task DirectCall_ShouldFail()
    {
        var sut = CreateMediator();
        var res = await sut.Dispatch(new ProtectedAction());
        Assert.False(res.Success);
        Assert.Equal(MediatorException.CreateForForbidenDirectCall().Message, res.GetErrorMessage());
    }

    private IMediator CreateMediator()
    {
        return Factory.CreateCustomMediator(c =>
        {
            c.AddActions(new Type[] { typeof(RootAction), typeof(ProtectedAction) });
            c.AddHandlers(new Type[] { typeof(RootActionHandler), typeof(ProtectedActionHandler) });
            c.UseWhen(cond => cond is IProtectedAction, m => m.UseDirectCallProtection());
        });
    }

    public class RootAction : IMediatorAction
    {
    }

    public class RootActionHandler : IMediatorHandler<RootAction>
    {
        private readonly IMediator _mediator;

        public RootActionHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(RootAction action, CancellationToken cancellationToken)
        {
            return _mediator.DispatchUnhandled(new ProtectedAction(), cancellationToken);
        }
    }

    public interface IProtectedAction
    {
    }

    public class ProtectedAction : IMediatorAction, IProtectedAction
    {
    }

    public class ProtectedActionHandler : IMediatorHandler<ProtectedAction>
    {
        public Task Handle(ProtectedAction action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}