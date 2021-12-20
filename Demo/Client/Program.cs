using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Http;
using Demo.Client;
using Demo.Shared;
using Demo.Shared.Requests;
using Demo.Client.Middlewares;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//////// Mediator implementation
builder.Services.AddMediatorClient(o =>
{
    o.Endpoint = Constants.CustomMediatorUrl;
    o.DeserializeOnlyCredibleResultTypes = true;
    o.CredibleResultTypes = new Type[]
    {
        typeof(CommonResult)
    };
})
    .AddActionsFromAssemblyOf<WeatherForecast.Request>()
    .AddPipeline<IRequest>()
        .UseReduceDuplicateProcessing()
        .Use<NotificationMiddleware>()
    .AddDefaultPipeline()
        .Use<NotificationMiddleware>();
////////

await builder.Build().RunAsync();