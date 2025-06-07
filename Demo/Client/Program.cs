using Blazored.LocalStorage;
using Demo.Client.Services;
using Demo.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
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
        o.IgnoreReadOnlyProperties = true;
        o.AddCredibleResultType<CommonResult>();
        o.AddContextAccessor = false; // Reduce overhead per every mediator call
    })
    .AddActionsFromAssemblyOf<WeatherForecast.Request>()
    .AddPipelineForAuthorizationRequest(p =>
    {
        p.Use<AuthCacheMediatorMiddleware>();
    })
    .UseNotificationReceiver()
    .UseWhenAction<IRequest>(s => s
        .UseReduceDuplicateProcessing()
        .Use<CancellationOnNavigationMediatorMiddleware>(ServiceLifetime.Singleton))
    .UseActionEvents();
////////

var host = builder.Build();
var runTask = host.RunAsync();

// Call JS to hide the spinner
await host.Services.GetRequiredService<IJSRuntime>()
    .InvokeVoidAsync("hideGlobalSpinner");
await runTask;