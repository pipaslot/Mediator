using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Internal;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Middlewares;

public class DirectHttpCallProtectionMiddlewareTests
{
    [Fact]
    public async Task RootHttpCall_FailureWithGenericErrorDueToMissingExceptionHandler()
    {
        var mediator = CreateMediator(featureSet: true);
        var res = await mediator.Dispatch(new ProtectedAction());

        Assert.False(res.Success);
        Assert.Equal(Mediator.GenericErrorMessage, res.GetErrorMessage());
    }

    [Fact]
    public async Task NestedCall_ShouldPass()
    {
        // Even though the outer HTTP request feature is set, the protected action is only reached
        // as a nested call (ContextStack.Count > 1), so IsExecutedFromPublicApi must be false for it.
        var mediator = CreateMediator(featureSet: true);
        var res = await mediator.Dispatch(new RootAction());

        Assert.True(res.Success);
    }

    [Fact]
    public async Task CallOutsideHttpRequest_ShouldPass()
    {
        // No MediatorHttpContextFeature set (e.g. call from a background service holding a stale
        // HttpContext, or no HTTP request at all) => not considered a public API call, guard does not trigger.
        var mediator = CreateMediator(featureSet: false);
        var res = await mediator.Dispatch(new ProtectedAction());

        Assert.True(res.Success);
    }

    private static IMediator CreateMediator(bool featureSet)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(CreateHttpContextAccessor(featureSet));
        services.AddMediatorServer()
            .AddActions([typeof(RootAction), typeof(ProtectedAction)])
            .AddHandlers([typeof(RootActionHandler), typeof(ProtectedActionHandler)])
            .UseWhen(action => action is IProtectedAction, m => m.UseDirectHttpCallProtection());
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(bool featureSet)
    {
        var features = new FeatureCollection();
        if (featureSet)
        {
            features.Set(MediatorHttpContextFeature.Instance);
        }

        var context = new Mock<HttpContext>();
        context.SetupGet(c => c.Features).Returns(features);

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupGet(a => a.HttpContext).Returns(context.Object);
        return accessor.Object;
    }

    public interface IProtectedAction;

    public class RootAction : IMediatorAction;

    public class RootActionHandler(IMediator mediator) : IMediatorHandler<RootAction>
    {
        public Task Handle(RootAction action, CancellationToken cancellationToken)
        {
            return mediator.DispatchUnhandled(new ProtectedAction(), cancellationToken);
        }
    }

    public class ProtectedAction : IMediatorAction, IProtectedAction;

    public class ProtectedActionHandler : IMediatorHandler<ProtectedAction>
    {
        public Task Handle(ProtectedAction action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
