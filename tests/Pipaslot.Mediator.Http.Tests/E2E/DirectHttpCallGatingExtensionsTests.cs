using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Internal;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests.E2E;

/// <summary>
/// Covers the <see cref="MiddlewareRegistratorExtensions"/> shortcuts that gate a sub-pipeline on whether the
/// current action is a direct HTTP call (<c>UseWhenDirectHttpCall</c>, <c>UseWhenNotDirectHttpCall</c>). Unlike
/// <c>MiddlewareRegistratorExtensionsTests</c>, these cannot be proven correct with a mocked
/// <see cref="IMiddlewareRegistrator"/> alone - the whole point is whether the condition they wire in actually
/// discriminates a root call from a nested one against a real call tree, so each test dispatches through a real
/// container with a probe middleware. The condition's own truth table (root/nested, with/without HTTP context) is
/// exhaustively covered in isolation by <c>Internal.HttpContextAccessorExtensionsTests</c>; the tests below only
/// confirm each extension wires that condition (or its negation) to the right sub-pipeline.
/// <see cref="MiddlewareRegistratorExtensions.UseAuthorizationWhenDirectHttpCall"/> shares the same condition but is
/// covered separately in <see cref="AuthorizationWhenDirectHttpCallTests"/>, since it exercises a distinct domain
/// (real authorization denial/bypass) rather than plain middleware ordering.
/// </summary>
public class DirectHttpCallGatingExtensionsTests
{
    [Fact]
    public async Task UseWhenDirectHttpCallGeneric_AppliesMiddlewareToRootActionOnly_WhenCalledFromHttpContext()
    {
        var log = new List<string>();
        var mediator = CreateProbeMediator(log, indicatePublicApiAccess: true, m => m.UseWhenDirectHttpCall<ProbeMiddleware>());

        await mediator.DispatchUnhandled(new RootAction());

        Assert.Equal([nameof(RootAction)], log);
    }

    [Fact]
    public async Task UseWhenDirectHttpCallGeneric_NeverApplies_WhenCalledOutsideHttpContext()
    {
        var log = new List<string>();
        var mediator = CreateProbeMediator(log, indicatePublicApiAccess: false, m => m.UseWhenDirectHttpCall<ProbeMiddleware>());

        await mediator.DispatchUnhandled(new LeafAction());

        Assert.Empty(log);
    }

    [Fact]
    public async Task UseWhenDirectHttpCallDelegate_AppliesEveryRegisteredMiddleware_WhenCalledFromHttpContext()
    {
        // Distinct from the generic overload above: proves the Action<IMiddlewareRegistrator> form can chain
        // multiple middlewares under the same single condition, not just wrap one Use<T>() call.
        var log = new List<string>();
        var mediator = CreateProbeMediator(log, indicatePublicApiAccess: true,
            m => m.UseWhenDirectHttpCall(sub => sub.Use<ProbeMiddleware>().Use<SecondProbeMiddleware>()));

        await mediator.DispatchUnhandled(new LeafAction());

        Assert.Equal([nameof(LeafAction), $"Second:{nameof(LeafAction)}"], log);
    }

    [Fact]
    public async Task UseWhenNotDirectHttpCallGeneric_AppliesMiddlewareToNestedActionOnly_WhenCalledFromHttpContext()
    {
        var log = new List<string>();
        var mediator = CreateProbeMediator(log, indicatePublicApiAccess: true, m => m.UseWhenNotDirectHttpCall<ProbeMiddleware>());

        await mediator.DispatchUnhandled(new RootAction());

        Assert.Equal([nameof(LeafAction)], log);
    }

    [Fact]
    public async Task UseWhenNotDirectHttpCallGeneric_Applies_WhenCalledOutsideHttpContext()
    {
        var log = new List<string>();
        var mediator = CreateProbeMediator(log, indicatePublicApiAccess: false, m => m.UseWhenNotDirectHttpCall<ProbeMiddleware>());

        await mediator.DispatchUnhandled(new LeafAction());

        Assert.Equal([nameof(LeafAction)], log);
    }

    private static IMediator CreateProbeMediator(List<string> log, bool indicatePublicApiAccess, Action<IMiddlewareRegistrator> configureMiddleware)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(log);
        services.AddSingleton(CreateHttpContextAccessor(indicatePublicApiAccess));
        var configurator = services.AddMediator()
            .AddActions([typeof(RootAction), typeof(LeafAction)])
            .AddHandlers([typeof(RootActionHandler), typeof(LeafActionHandler)]);
        configureMiddleware(configurator);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(bool indicatePublicApiAccess)
    {
        var features = new FeatureCollection();
        if (indicatePublicApiAccess)
        {
            features.Set(MediatorHttpContextFeature.Instance);
        }

        var context = new Mock<HttpContext>();
        context.SetupGet(c => c.Features).Returns(features);

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupGet(a => a.HttpContext).Returns(context.Object);
        return accessor.Object;
    }

    private class ProbeMiddleware(List<string> log) : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            log.Add(context.Action.GetType().Name);
            return next(context);
        }
    }

    private class SecondProbeMiddleware(List<string> log) : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            log.Add($"Second:{context.Action.GetType().Name}");
            return next(context);
        }
    }

    public class RootAction : IMediatorAction;

    public class RootActionHandler(IMediator mediator) : IMediatorHandler<RootAction>
    {
        public Task Handle(RootAction action, CancellationToken cancellationToken)
        {
            return mediator.DispatchUnhandled(new LeafAction(), cancellationToken);
        }
    }

    public class LeafAction : IMediatorAction;

    public class LeafActionHandler : IMediatorHandler<LeafAction>
    {
        public Task Handle(LeafAction action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
