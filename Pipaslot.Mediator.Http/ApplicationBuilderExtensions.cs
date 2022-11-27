using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Services;

namespace Pipaslot.Mediator.Http
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Register Mediator middleware handling messages
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="checkMatchingHandlers">Scans for all action markers and make sure that all of them have the appropriate amount of handlers registered during application startup</param>
        /// <param name="checkExistingPolicies">Check that every action or action handler has at least one Authorization policy to prevent runtime exceptions</param>
        public static IApplicationBuilder UseMediator(this IApplicationBuilder app, bool checkMatchingHandlers = false, bool checkExistingPolicies = false)
        {
            var options = app.ApplicationServices.GetService<ServerMediatorOptions>() ?? new ServerMediatorOptions();
            app.UseMiddleware<MediatorMiddleware>(options);

            if (checkMatchingHandlers || checkExistingPolicies)
            {
                using var scope = app.ApplicationServices.CreateScope();
                var checker = scope.ServiceProvider.GetRequiredService<IHandlerExistenceChecker>();
                checker.Verify(checkMatchingHandlers, checkExistingPolicies);
            }
            return app;
        }
    }
}
