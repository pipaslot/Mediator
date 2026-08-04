using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Middlewares;

/// <summary>
/// Contexts synthesized by <see cref="MediatorContext.Create"/>, the seam a consumer uses to invoke a middleware or
/// exception handler directly. <see cref="MediatorContextTests"/> covers what holds for a context no matter how it
/// was built; this class covers only what the factory itself decides - the defaults standing in for dependencies the
/// pipeline would otherwise supply, and how a context reports a dependency that was left out.
/// </summary>
public class MediatorContext_CreateTests
{
    [Fact]
    public void Create_WithoutServices_GetHandlersThrowsMediatorException()
    {
        var sut = MediatorContext.Create(new SingleHandler.Message(true));

        var exception = Assert.Throws<MediatorException>(() => sut.GetHandlers());

        Assert.Contains(nameof(IServiceProvider), exception.Message);
    }

    [Fact]
    public async Task Create_WithoutMediator_NestedCallThrowsMediatorException()
    {
        var sut = MediatorContext.Create(new SingleHandler.Message(true));
        var nested = new SingleHandler.Message(true);

        var exception = await Assert.ThrowsAsync<MediatorException>(() => sut.Mediator.Dispatch(nested));

        Assert.Same(nested, exception.Data["action"]);
    }

    [Fact]
    public void Create_WithServices_MediatorIsResolvedFromServices()
    {
        var services = Factory.CreateServiceProvider();

        var sut = MediatorContext.Create(new SingleHandler.Message(true), services);

        Assert.Same(services.GetRequiredService<IMediator>(), sut.Mediator);
    }

    [Fact]
    public void Create_WithBothMediatorAndServices_MediatorArgumentWins()
    {
        var services = Factory.CreateServiceProvider();
        var mediator = Substitute.For<IMediator>();

        var sut = MediatorContext.Create(new SingleHandler.Message(true), services, mediator);

        Assert.Same(mediator, sut.Mediator);
    }

    [Fact]
    public void Create_WithServices_GetHandlersResolvesRegisteredHandler()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.MessageHandler>();

        var sut = MediatorContext.Create(new SingleHandler.Message(true), services);

        Assert.IsType<SingleHandler.MessageHandler>(Assert.Single(sut.GetHandlers()));
    }

    [Fact]
    public void Create_WithoutDepth_IsNotNested()
    {
        var sut = MediatorContext.Create(new SingleHandler.Message(true));

        Assert.False(sut.IsNested);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void Create_DepthGreaterThanOne_IsNested(int depth)
    {
        var sut = MediatorContext.Create(new SingleHandler.Message(true), depth: depth);

        Assert.True(sut.IsNested);
    }

    [Fact]
    public void Create_DepthBelowOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MediatorContext.Create(new SingleHandler.Message(true), depth: 0));
    }

    [Fact]
    public void Create_DepthGreaterThanOne_ParentContextsStayEmpty()
    {
        var sut = MediatorContext.Create(new SingleHandler.Message(true), depth: 2);

        Assert.Empty(sut.ParentContexts);
    }

    [Fact]
    public void Create_Features_ContainDefaultMiddlewareParameters()
    {
        var sut = MediatorContext.Create(new SingleHandler.Message(true));

        Assert.NotNull(sut.Features.Get<MiddlewareParametersFeature>());
    }

    [Fact]
    public async Task Create_PassedToMiddlewareInvoke_ContextRecordsWhatMiddlewareDid()
    {
        var sut = MediatorContext.Create(new SingleHandler.Message(true));
        var middleware = new ResultAppendingMiddleware();

        await middleware.Invoke(sut, Factory.CreateMiddlewareDelegate());

        Assert.Same(ResultAppendingMiddleware.Result, Assert.Single(sut.Results));
    }

    private class ResultAppendingMiddleware : IMediatorMiddleware
    {
        public static readonly object Result = new();

        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddResult(Result);

            return next(context);
        }
    }
}
