using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Handlers;
using Pipaslot.Mediator.Notifications;
using Pipaslot.Mediator.Services;

namespace Pipaslot.Mediator;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures handler sources and pipeline for handler processing.
    /// Every Request/Message is configured to have exactly one handler by default.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="addContextAccessor">Register <see cref="IMediatorContextAccessor"/> and <see cref="INotificationProvider"/> needed for context accessing out of the mediator middlewares</param>
    public static IMediatorConfigurator AddMediator(this IServiceCollection services, bool addContextAccessor = true)
    {
        return services.AddMediator<HandlerExecutionMiddleware>(addContextAccessor);
    }

    /// <summary>
    /// Configures handler sources and pipeline for handler processing.
    /// Every Request/Message is configured to have exactly one handler by default.
    /// </summary>
    /// <typeparam name="TDefaultExecutionMiddleware">Default handler executive middleware used in case when no other middleware is registered</typeparam>
    /// <param name="services"></param>
    /// <param name="addContextAccessor">Register <see cref="IMediatorContextAccessor"/> and <see cref="INotificationProvider"/> needed for context accessing out of the mediator middlewares</param>
    public static IMediatorConfigurator AddMediator<TDefaultExecutionMiddleware>(this IServiceCollection services, bool addContextAccessor = true)
        where TDefaultExecutionMiddleware : class, IExecutionMiddleware
    {
        if (addContextAccessor)
        {
            services.AddScoped<MediatorContextAccessor>();
            services.AddScoped<IMediatorContextAccessor>(s => s.GetRequiredService<MediatorContextAccessor>());
            services.AddScoped<INotificationProvider>(s => s.GetRequiredService<MediatorContextAccessor>());
            services.AddScoped<IMediatorFacade, MediatorFacade>();
        }
        else
        {
            services.AddScoped<INotificationProvider, NotificationReceiverMiddlewarePropagator>();
        }
        var configurator = new MediatorConfigurator(services);
        services.AddSingleton(configurator);
        services.AddScoped<IMediator>(s =>
        {
            var mca = s.GetService<MediatorContextAccessor>();// Optional
            var logger = s.GetService<ILogger<Mediator>>() ?? NullLogger<Mediator>.Instance;// Optional, no-op when no logging provider is configured
            return new Mediator(s, mca, configurator, logger);
        });
        services.AddTransient<IHandlerExistenceChecker, HandlerExistenceChecker>();
        services.AddSingleton<IActionTypeProvider>(configurator);
        services.AddScoped<IExecutionMiddleware, TDefaultExecutionMiddleware>();
        services.AddScoped<IClaimPrincipalAccessor, ClaimPrincipalAccessor>();
        services.AddTransient(typeof(MessageHandlerExecutor<>));
        services.AddTransient(typeof(RequestHandlerExecutor<,>));
        services.AddTransient(typeof(ExceptionHandlerExecutor<>));
        configurator.AddActions([typeof(AuthorizeRequest)]);
        configurator.AddHandlers([typeof(AuthorizeRequestHandler)]);

        // Separate authorization middleware, because we do not want to interrupt by custom middlewares
        configurator.AddPipelineForAuthorizationRequest(static _ => { });
        services.AddScoped<INodeFormatter, DefaultNodeFormatter>();
        
        services.AddScoped<NotificationReceiverMiddleware>();
        services.AddScoped<INotificationReceiver>(s => s.GetRequiredService<NotificationReceiverMiddleware>());
        return configurator;
    }

    /// <summary>
    /// Replaces the <see cref="INodeFormatter"/> registered by <see cref="AddMediator(IServiceCollection, bool)"/> (<see cref="DefaultNodeFormatter"/> by default)
    /// with a custom implementation, instead of adding a second, competing registration.
    /// Must be called after <see cref="AddMediator(IServiceCollection, bool)"/>.
    /// </summary>
    public static IServiceCollection ReplaceNodeFormatter<TNodeFormatter>(this IServiceCollection services)
        where TNodeFormatter : class, INodeFormatter
    {
        services.Replace(ServiceDescriptor.Scoped<INodeFormatter, TNodeFormatter>());
        return services;
    }
}