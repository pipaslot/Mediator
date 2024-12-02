using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Services;
using System;

namespace Pipaslot.Mediator.Http.Web;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Register Mediator middleware handling messages
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="checkMatchingHandlers">Scans for all action markers and make sure that all of them have the appropriate amount of handlers registered during application startup</param>
    /// <param name="checkExistingPolicies">Check that every action or action handler has at least one Authorization policy to prevent runtime exceptions</param>
    public static IApplicationBuilder UseMediator(this IApplicationBuilder app, bool checkMatchingHandlers = false,
        bool checkExistingPolicies = false)
    {
        ClearTemporaryConfigurationData(app);
        RegisterMiddleware(app);
        ExecuteChecker(app,
            new ExistenceCheckerSetting { CheckMatchingHandlers = checkMatchingHandlers, CheckExistingPolicies = checkExistingPolicies });
        return app;
    }

    /// <summary>
    /// Register Mediator middleware handling messages
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="checkMatchingHandlers">Scans for all action markers and make sure that all of them have the appropriate amount of handlers registered during application startup</param>
    /// <param name="ignoredPolicyCheckTypes">If set, check that every action or action handler has at least one Authorization policy to prevent runtime exceptions. All the specified types will be ignored during the check.</param>
    public static IApplicationBuilder UseMediator(this IApplicationBuilder app, bool checkMatchingHandlers, params Type[] ignoredPolicyCheckTypes)
    {
        ClearTemporaryConfigurationData(app);
        RegisterMiddleware(app);
        ExecuteChecker(app,
            new ExistenceCheckerSetting
            {
                CheckMatchingHandlers = checkMatchingHandlers,
                CheckExistingPolicies = true,
                IgnoredPolicyChecks = [..ignoredPolicyCheckTypes]
            });
        return app;
    }

    /// <summary>
    /// Clear data applicable only during mediator configuration. They does not need to be held during runetime.
    /// </summary>
    private static void ClearTemporaryConfigurationData(IApplicationBuilder app)
    {
        var conf = app.ApplicationServices.GetRequiredService<MediatorConfigurator>();
        conf.ClearTempData();
    }

    private static void RegisterMiddleware(IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService<ServerMediatorOptions>() ?? new ServerMediatorOptions();
        app.UseMiddleware<MediatorMiddleware>(options);
    }

    private static void ExecuteChecker(IApplicationBuilder app, ExistenceCheckerSetting setting)
    {
        if (setting.CheckMatchingHandlers || setting.CheckExistingPolicies)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var checker = scope.ServiceProvider.GetRequiredService<IHandlerExistenceChecker>();
            checker.Verify(setting);
        }
    }
}