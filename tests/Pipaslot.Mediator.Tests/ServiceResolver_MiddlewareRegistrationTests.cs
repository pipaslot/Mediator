using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests;

public class ServiceResolver_MiddlewareRegistrationTests
{
    [Fact]
    public void Use_AddTheSameMiddlewareWithDifferentLifetimeOntTheSamePipeline_ThrowException()
    {
        Assert.Throws<MediatorException>(() =>
        {
            Factory.CreateServiceProvider(c => c
                .Use<FakeMiddleware>(ServiceLifetime.Scoped)
                .Use<FakeMiddleware>(ServiceLifetime.Transient)
            );
        });
    }

    [Fact]
    public void Use_AddTheSameMiddlewareWithDifferentLifetimeOntDifferentePipeline_ThrowException()
    {
        Assert.Throws<MediatorException>(() =>
        {
            Factory.CreateServiceProvider(c => c
                .Use<FakeMiddleware>(ServiceLifetime.Scoped)
                .UseWhenAction<IMessage>(s => s.Use<FakeMiddleware>(ServiceLifetime.Transient))
            );
        });
    }

    [Fact]
    public void Use_AddTheSameMiddlewareWithDifferentLifetimeOntDifferentePipeline2_ThrowException()
    {
        Assert.Throws<MediatorException>(() =>
        {
            Factory.CreateServiceProvider(c => c
                .Use<FakeMiddleware>(ServiceLifetime.Scoped)
                .UseWhen((a, s) => true, s => s.Use<FakeMiddleware>(ServiceLifetime.Transient))
            );
        });
    }

    private class FakeMiddleware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            throw new System.NotImplementedException();
        }
    }
}