﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
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
    public static IMediatorConfigurator AddMediator(this IServiceCollection services)
    {
        return services.AddMediator<HandlerExecutionMiddleware>();
    }

    /// <summary>
    /// Configures handler sources and pipeline for handler processing.
    /// Every Request/Message is configured to have exactly one handler by default.
    /// </summary>
    /// <typeparam name="TDefaultExecutionMiddleware">Default handler executive middleware ised in case when no other middleware is registered</typeparam>
    /// <param name="services"></param>
    public static IMediatorConfigurator AddMediator<TDefaultExecutionMiddleware>(this IServiceCollection services)
        where TDefaultExecutionMiddleware : class, IExecutionMiddleware
    {
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<MediatorContextAccessor>();
        services.AddScoped<IMediatorContextAccessor>(s => s.GetRequiredService<MediatorContextAccessor>());
        services.AddTransient<IHandlerExistenceChecker, HandlerExistenceChecker>();
        var configurator = new MediatorConfigurator(services);
        services.AddSingleton(configurator);
        services.AddSingleton<IActionTypeProvider>(configurator);
        services.AddScoped<IExecutionMiddleware, TDefaultExecutionMiddleware>();
        services.AddScoped<INotificationProvider>(s => s.GetRequiredService<MediatorContextAccessor>());
        services.AddScoped<IMediatorFacade, MediatorFacade>();
        services.AddScoped<IClaimPrincipalAccessor, ClaimPrincipalAccessor>();
        configurator.AddActions([typeof(AuthorizeRequest)]);
        configurator.AddHandlers([typeof(AuthorizeRequestHandler)]);

        // Separate authorization middleware, because we do not want to interrupt by custom middlewares
        configurator.AddPipelineForAuthorizationRequest(_ => { });
        services.AddScoped<INodeFormatter, DefaultNodeFormatter>();
        return configurator;
    }
}