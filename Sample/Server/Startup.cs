using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Server;
using Sample.Server.RequestHandlers;
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
            services.AddMediator(o=> {
                o.Endpoint = Constants.CustomMediatorUrl;
            })
                .AddActionsFromAssemblyOf<WeatherForecast.Request>()
                .AddHandlersFromAssemblyOf<WheatherForecastRequestHandler>()
                .Use<CustomMediatorMiddleware>();
                //.Use<CommandSpecificMiddleware, ICommand>()
                //.Use<QuerySpecificMiddleware, IQuery>()
                //.UseSequenceMultiHandler<ICommand>();
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
