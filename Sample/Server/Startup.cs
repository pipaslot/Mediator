using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pipaslot.Mediator.Server;
using Sample.Server.Handlers;
using Sample.Shared;
using Sample.Shared.Requests;

namespace Sample.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddRazorPages();

            //////// Mediator implementation
            services.AddMediatorServer(o=> {
                o.Endpoint = Constants.CustomMediatorUrl;
            })
                .AddActionsFromAssemblyOf<WeatherForecast.Request>()
                .AddHandlersFromAssemblyOf<WheatherForecastRequestHandler>()
                // Configure pipelines for own custom action types. This is CQRS implementaiton sample 
                //.AddPipeline<IQuery>()                // Pipeline specified only for queries
                //    .UseExceptionLogging()           // Log all unhalded exception via ILogger
                //    .Use<QuerySpecificMiddleware>()   // Middleare which should be applied only to Queries
                //    .Use<CommonMiddleware>()          // Middleware which should be used for all action types

                //.AddPipeline<ICommand>()              // Pipeline specified only for commands
                //    .UseExceptionLogging()           // Log all unhalded exception via ILogger
                //    .Use<CommandSpecificMiddleware>() // Middleare which should be applied only to Commands
                //    .Use<CommonMiddleware>()          // Middleware which should be used for all action types
                //    .UseSequenceMultiHandler()        // This allow to define and executie multiple action handlers. Must be defined as last middleware in pipeline. 

                // Use default pipelin if you do not use Action specific specific middlewares or any from previous pipelines does not fullfil condition for execution
                .AddDefaultPipeline()                   // Pipeline for all action not handled by any of previous pipelines
                    .UseExceptionLogging()             // Log all unhalded exception via ILogger
                    .Use<ValidatorMiddleware>()
                    .Use<CommonMiddleware>();
            ////////
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            //////// Mediator implementation
            app.UseMediator(env.IsDevelopment());
            ////////
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
