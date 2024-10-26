using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Pipaslot.Mediator.Tests.E2E;

public class MiddlewareParametersFeatureTests
{
    [Fact]
    public async Task ParametricMiddleware_PropagateParameter()
    {
        var sut = Factory.CreateCustomMediator(s =>
        {
            s.UseWithParameters<AssertStringMiddleware>(AssertStringMiddleware.Value);
        });
        await sut.DispatchUnhandled(new NopMessage());
    }

    [Fact]
    public async Task ParametricMiddleware_FailBecauseMissingExpectedParameter()
    {
        var sut = Factory.CreateCustomMediator(s =>
        {
            s.Use<AssertNoParameterMiddleware>();
        });
        await sut.DispatchUnhandled(new NopMessage());
    }

    [Fact]
    public async Task ParametersAreAlwaysReset()
    {
        var sut = Factory.CreateCustomMediator(s =>
        {
            s.UseWithParameters<AssertAndChangeMiddleware>(new MutableParameter());
            s.UseWithParameters<AssertAndChangeMiddleware>(new MutableParameter());
            s.Use<AssertNoParameterMiddleware>();
        });
        await sut.DispatchUnhandled(new NopMessage());
    }

    private class AssertStringMiddleware : IMediatorMiddleware
    {
        public static string Value = "string 1";

        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var actual = context.Features.Get<MiddlewareParametersFeature>()?.Parameters.FirstOrDefault();
            Assert.NotNull(actual);
            Assert.Equal(Value, actual);
            return Task.CompletedTask; // Do not call await next(context) to keep the test simple without need to register handlers
        }
    }

    private class AssertNoParameterMiddleware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var parameters = context.Features.Get<MiddlewareParametersFeature>()?.Parameters;
            Assert.NotNull(parameters);
            Assert.Empty(parameters);
            return Task.CompletedTask; // Do not call await next(context) to keep the test simple without need to register handlers
        }
    }

    private class AssertAndChangeMiddleware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var actual = context.Features.Get<MiddlewareParametersFeature>()?.Parameters.FirstOrDefault() as MutableParameter;
            Assert.NotNull(actual);
            Assert.Equal(MutableParameter.DefaultValue, actual.Value);
            actual.Mutate();
            return Task.CompletedTask; // Do not call await next(context) to keep the test simple without need to register handlers
        }
    }

    private class MutableParameter
    {
        public const string DefaultValue = "val1";
        public string Value { get; set; } = DefaultValue;

        public void Mutate()
        {
            Value = Guid.NewGuid().ToString();
        }
    }
}