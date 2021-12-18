using Pipaslot.Mediator.Middlewares;
using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class ServiceResolver_MiddlewareRegistrationTests
    {
        [Fact]
        public void Use_AddTheSameMiddlewareWithDifferentLifetimeOntTheSamePipeline_ThrowException()
        {
            Assert.Throws<MediatorException>(() =>
            {
                Factory.CreateServiceProvider(c => c
                .AddDefaultPipeline()
                    .Use<MultiHandlerSequenceExecutionMiddleware>(ServiceLifetime.Scoped)
                    .Use<MultiHandlerSequenceExecutionMiddleware>(ServiceLifetime.Transient)
                );
            });
        }
        [Fact]
        public void Use_AddTheSameMiddlewareWithDifferentLifetimeOntDifferentePipeline_ThrowException()
        {
            Assert.Throws<MediatorException>(() =>
            {
                Factory.CreateServiceProvider(c => c
                .AddDefaultPipeline()
                    .Use<MultiHandlerSequenceExecutionMiddleware>(ServiceLifetime.Scoped)
                .AddPipeline<IMessage>()
                    .Use<MultiHandlerSequenceExecutionMiddleware>(ServiceLifetime.Transient)
                );
            });
        }
    }
}
