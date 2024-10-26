using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;
using System;

namespace Pipaslot.Mediator.Http.Tests
{
    internal class Factory
    {
        public static IMediator CreateMediator(Action<IMediatorConfigurator> setup)
        {
            var services = CreateServiceProvider(c =>
                {
                    c.AddActionsFromAssemblyOf<Factory>()
                        .AddActionsFromAssemblyOf<SingleHandler.Message>()
                        .AddHandlersFromAssemblyOf<Factory>()
                        .AddHandlersFromAssemblyOf<SingleHandler.MessageHandler>();
                    ;
                    setup(c);
                }
            );
            return services.GetRequiredService<IMediator>();
        }

        public static IServiceProvider CreateServiceProvider(Action<IMediatorConfigurator> setup)
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            setup(collection.AddMediator());
            return collection.BuildServiceProvider();
        }
    }
}