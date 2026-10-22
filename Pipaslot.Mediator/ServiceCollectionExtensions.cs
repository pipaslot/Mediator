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

/// <summary>
/// Registers the in-process mediator in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures handler sources and pipeline for handler processing.
    /// Every Request/Message is configured to have exactly one handler by default.
    /// </summary>
    /// <remarks>
    /// The entry point for running actions in the same process as their handlers. For the client/server setup over HTTP,
    /// the Pipaslot.Mediator.Http package provides <c>AddMediatorServer</c> and <c>AddMediatorClient</c>, which call this
    /// method themselves - use them instead, not in addition. Register actions, handlers and middlewares through the
    /// returned <see cref="IMediatorConfigurator"/>; call this once per service collection.
    /// </remarks>
    /// <param name="services">The service collection to register the mediator in</param>
    /// <param name="addContextAccessor">Register <see cref="IMediatorContextAccessor"/> and <see cref="INotificationProvider"/> needed for context accessing out of the mediator middlewares</param>
    public static IMediatorConfigurator AddMediator(this IServiceCollection services, bool addContextAccessor = true)
    {
        return services.AddMediator<HandlerExecutionMiddleware>(addContextAccessor);
    }

    /// <summary>
    /// Configures handler sources and pipeline for handler processing.
    /// Every Request/Message is configured to have exactly one handler by default.
    /// </summary>
    /// <remarks>
    /// Use this overload to replace how the end of every pipeline executes an action - <c>AddMediatorClient</c> passes its
    /// HTTP execution middleware here, so actions are sent to the server instead of to local handlers. Use
    /// <see cref="AddMediator(IServiceCollection, bool)"/> to run the registered handlers.
    /// </remarks>
    /// <typeparam name="TDefaultExecutionMiddleware">Default handler executive middleware used in case when no other middleware is registered</typeparam>
    /// <param name="services">The service collection to register the mediator in</param>
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
#pragma warning disable CS0618 // Internal use of a type that becomes internal in 10.0.
        services.AddTransient<IHandlerExistenceChecker, HandlerExistenceChecker>();
        services.AddSingleton<IActionTypeProvider>(configurator.ReflectionCache);
#pragma warning restore CS0618
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