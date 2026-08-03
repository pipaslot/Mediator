using Pipaslot.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.Pipeline;

public class DirectCallProtectionTests
{
    [Fact]
    public async Task IndirectCall_ShouldPass()
    {
        var sut = CreateMediator();
        var res = await sut.Dispatch(new RootAction());
        Assert.True(res.Success);
    }

    [Fact]
    public async Task DirectCall_FailureWithGenericErrorDueToMissingExceptionHandler()
    {
        var sut = CreateMediator();
        var res = await sut.Dispatch(new ProtectedAction());
        Assert.False(res.Success);
        Assert.Equal(Mediator.GenericErrorMessage, res.GetErrorMessage());
    }

    private IMediator CreateMediator()
    {
        return Factory.CreateMediator(c =>
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