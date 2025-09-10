using Pipaslot.Mediator.Abstractions;
using System.Threading;

namespace Pipaslot.Mediator.Tests.Middlewares;

public class DirectCallProtectionMiddlewareTests
{
    [Test]
    public async Task IndirectCall_ShouldPass()
    {
        var sut = CreateMediator();
        var res = await sut.Dispatch(new RootAction());
        Assert.True(res.Success);
    }

    [Test]
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
            c.AddActions([typeof(RootAction), typeof(ProtectedAction)]);
            c.AddHandlers([typeof(RootActionHandler), typeof(ProtectedActionHandler)]);
            c.UseWhen(cond => cond is IProtectedAction, m => m.UseDirectCallProtection());
        });
    }

    public class RootAction : IMediatorAction;

    public class RootActionHandler(IMediator mediator) : IMediatorHandler<RootAction>
    {
        public Task Handle(RootAction action, CancellationToken cancellationToken)
        {
            return mediator.DispatchUnhandled(new ProtectedAction(), cancellationToken);
        }
    }

    public interface IProtectedAction;

    public class ProtectedAction : IMediatorAction, IProtectedAction;

    public class ProtectedActionHandler : IMediatorHandler<ProtectedAction>
    {
        public Task Handle(ProtectedAction action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}