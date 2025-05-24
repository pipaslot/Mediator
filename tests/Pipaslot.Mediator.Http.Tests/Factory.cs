using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.IO;
using System.Text;

namespace Pipaslot.Mediator.Http.Tests;

internal static class Factory
{
    public static IMediator CreateMediator(Action<IMediatorConfigurator> setup)
    {
        var services = CreateServiceProvider(c =>
            {
                c.AddActionsFromAssembly(typeof(Factory).Assembly)
                    .AddActionsFromAssemblyOf<SingleHandler.Message>()
                    .AddHandlersFromAssembly(typeof(Factory).Assembly)
                    .AddHandlersFromAssemblyOf<SingleHandler.MessageHandler>();
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