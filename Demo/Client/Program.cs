using Demo.Client;
using Demo.Shared;
using Demo.Shared.Requests;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//////// Mediator implementation
builder.Services.AddMediatorClient(o =>
{
    o.Endpoint = Constants.CustomMediatorUrl;
    o.DeserializeOnlyCredibleResultTypes = true;
    o.SerializerTyoe = SerializerType.V3;
    o.AddCredibleResultType<CommonResult>();
})
    .AddActionsFromAssemblyOf<WeatherForecast.Request>()
    .UseNotificationReceiver()
    .UseWhenAction<IRequest>(s => s.UseReduceDuplicateProcessing())
    .UseActionEvents();
////////

await builder.Build().RunAsync();