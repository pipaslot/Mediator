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
        public static IApplicationBuilder UseMediator(this IApplicationBuilder app, bool checkMatchingHandlers = false)
        {
            var options = app.ApplicationServices.GetService<ServerMediatorOptions>() ?? new ServerMediatorOptions();
            app.UseMiddleware<MediatorMiddleware>(options);
            if (checkMatchingHandlers)
            {
                using var scope = app.ApplicationServices.CreateScope();
                var checker = scope.ServiceProvider.GetRequiredService<IHandlerExistenceChecker>();
                checker.Verify();
            }
            return app;
        }
    }
}
