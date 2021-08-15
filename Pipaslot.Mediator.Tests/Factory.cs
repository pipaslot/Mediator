using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Services;
using System;

namespace Pipaslot.Mediator.Tests
{
    internal class Factory
    {
        public static IMediator CreateMediator()
        {
            return CreateMediator(c => { });
        }
        public static IMediator CreateMediator(Action<IPipelineConfigurator> setup)
        {
            var services = CreateServiceProvider(c =>
            {
                c.AddActionsFromAssemblyOf<Factory>()
                .AddHandlersFromAssemblyOf<Factory>();
                setup(c);
            }
            );
            return services.GetRequiredService<IMediator>();
        }

        public static ServiceResolver CreateServiceResolver(Action<IPipelineConfigurator> setup)
        {
            var services = CreateServiceProvider(setup);
            return services.GetRequiredService<ServiceResolver>();
        }

        public static ServiceProvider CreateServiceProvider(Action<IPipelineConfigurator> setup)
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            setup(collection.AddMediator());
            return collection.BuildServiceProvider();
        }
    }
}
