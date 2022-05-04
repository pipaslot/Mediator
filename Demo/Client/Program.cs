using Blazored.LocalStorage;
using Demo.Client;
using Demo.Client.Services;
using Demo.Shared;
using Demo.Shared.Requests;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
var services = builder.Services;
services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
services.AddBlazoredLocalStorage();
services.AddAuthorizationCore();
services.AddScoped<AuthService>();
services.AddScoped<AuthenticationStateProvider, AuthService>(provider => provider.GetRequiredService<AuthService>());

//////// Mediator implementation
services.AddMediatorClient(o =>
{
    o.Endpoint = Constants.CustomMediatorUrl;
    o.SerializerType = SerializerType.V3;
    o.AddCredibleResultType<CommonResult>();
})
    .AddActionsFromAssemblyOf<WeatherForecast.Request>()
    .UseNotificationReceiver()
    .UseWhenAction<IRequest>(s => s.UseReduceDuplicateProcessing())
    .UseActionEvents();
////////

await builder.Build().RunAsync();