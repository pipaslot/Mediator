using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Pipaslot.Mediator.Server
{
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Register Mediator middleware handling messages
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="checkMatchingHandlers">Scans for all action markers and make sure that all of them have the appropriate amount of handlers registered during application startup</param>
        public static IApplicationBuilder UseMediator(this IApplicationBuilder app, bool checkMatchingHandlers = false)
        {
            app.UseMiddleware<MediatorMiddleware>();
            if (checkMatchingHandlers)
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var checker = scope.ServiceProvider.GetRequiredService<HandlerExistenceChecker>();
                    checker.Verify();
                }
            }
            return app;
        }
    }
}
