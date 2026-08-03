using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

internal static class Factory
{
    #region Mediator
    
    public static IMediator CreateMediatorWithHandlers<THandler>()
    {
        var services = CreateServiceProvider(c =>
            {
                c.AddHandlers([typeof(THandler)]);
            }
        );
        return services.GetRequiredService<IMediator>();
    }

    public static Mediator GetConcreteMediator(this IServiceProvider sp) => (Mediator)sp.GetRequiredService<IMediator>();

    /// <summary>
    /// Same fixture wiring as <see cref="CreateCustomMediator"/>, plus a <see cref="TestLogger{T}"/> registered as
    /// <c>ILogger&lt;Mediator&gt;</c> so Execute/Dispatch boundary logging (level, exception, message) can be
    /// asserted directly instead of only inferred from Results.
    /// </summary>
    public static (IMediator Mediator, TestLogger<Mediator> Logger) CreateConfiguredMediatorWithLogger(Action<IMediatorConfigurator>? setup = null)
    {
        var logger = new TestLogger<Mediator>();
        var services = CreateServiceProvider((c, sc) =>
            {
                setup?.Invoke(c);
                sc.AddSingleton<ILogger<Mediator>>(logger);
            }
        );
        return (services.GetRequiredService<IMediator>(), logger);
    }

    public static IMediator CreateCustomMediator(Action<IMediatorConfigurator> setup)
    {
        var services = CreateServiceProvider(c =>
        {
            setup(c);
        });
        return services.GetRequiredService<IMediator>();
    }
    
    #endregion
    
    #region ServiceProvider

    public static IServiceProvider CreateServiceProvider() => CreateServiceProvider(_ => { });

    public static IServiceProvider CreateServiceProvider(Action<IMediatorConfigurator> setup)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        setup(collection.AddMediator());
        return collection.BuildServiceProvider();
    }

    public static IServiceProvider CreateServiceProvider(Action<IMediatorConfigurator, IServiceCollection> setup)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        setup(collection.AddMediator(), collection);
        return collection.BuildServiceProvider();
    }

    public static IServiceProvider CreateServiceProviderWithHandlers<THandler1>() => CreateServiceProviderWithHandlers(typeof(THandler1));

    public static IServiceProvider CreateServiceProviderWithHandlers<THandler1, THandler2>() =>
        CreateServiceProviderWithHandlers(typeof(THandler1), typeof(THandler2));

    /// <summary>
    /// Simulate handler registration in service provider
    /// </summary>
    public static IServiceProvider CreateServiceProviderWithHandlers(params Type[] handlers) =>
        CreateServiceProvider((m, s) =>
        {
            s.RegisterHandlers(new Dictionary<Type, ServiceLifetime>(), handlers);
        });
    #endregion

    public static MiddlewareDelegate CreateMiddlewareDelegate() => ctx => Task.CompletedTask;

    #region Context
    
    public static MediatorContext FakeContext(IMediatorAction action)
    {
        var services = CreateServiceProvider();
        var mediator = new Mock<IMediator>();
        var mcaMock = new Mock<IMediatorContextAccessor>();
        return new MediatorContext(mediator.Object, mcaMock.Object, services, new ReflectionCache(), action, CancellationToken.None, null, null);
    }

    public static MediatorContext CreateMediatorContext(this IServiceProvider services, IMediatorAction action)
    {
        var mediator = services.GetRequiredService<IMediator>();
        var ca = services.GetRequiredService<IMediatorContextAccessor>();
        return new MediatorContext(mediator, ca, services, new ReflectionCache(),action, CancellationToken.None, null, null);
    }
    
    #endregion
}