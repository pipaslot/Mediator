# Mediator
Mediator pattern over HTTP for Blazor WASM.
Define action contract on a shared project, define action handler on server-side and fire the action from the client. The mediator will carry about all of the communication for you.

This package was designed for fast and flexible development of simultaneously developed client and server applications.

## Basic usage

Define action contract on project shared between your client and server apps:
```
public static class WeatherForecast
{
    public class Request : IRequest<Result[]>
    {

    }

    public class Result
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string Summary { get; set; }
    }
}
```
Define action handler:
```
public class WheatherForecastRequestHandler : IRequestHandler<WeatherForecast.Request, WeatherForecast.Result[]>
{

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<WeatherForecast.Result[]> Handle(WeatherForecast.Request request, CancellationToken cancellationToken)
    {
        var rng = new Random();
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast.Result
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        })
        .ToArray();
        return Task.FromResult(forecast);
    }
}
```
and execute on your client:
```
@inject IMediator Mediator

@code {
    protected override async Task OnInitializedAsync()
    {
        var result = await Mediator.Execute(new WeatherForecast.Request());
        if (result.Success)
        {
            var forecasts = result.Result;
            ...
        }
    }

}
```

## Blazor Configuration
### Client-side
Program.cs
```
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    ...
    // Configure HTTP Client
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

    // Register Mediator services
    builder.Services.AddMediatorClient();

    ...
}
        
```
### Server-side
        
```
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddMediator()
            .AddActionsFromAssemblyOf<WeatherForecast.Request>()            //Scan for all action contracts from the same assembly as WeatherForecast
            .AddHandlersFromAssemblyOf<WheatherForecastRequestHandler>();   //Scan for all action handlers from the same assembly as WeatherForecast
        ...
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        ...
        app.UseMediator(env.IsDevelopment()); //Does also automatic check if all contracts has matching handlers during startup in development mode
        ...
    }
}
        
```
## Mediator
Execution of any action is made through `IMediator` interface (does not matter if it is from the client-side or server-side). This interface provides basic two methods `Dispatch` and `Execute`
`IMediator.Dispatch` - Executes messages without expected response. It is like fire and forget. But still you can await this action because server can for example refuse this request for many reasons (like validation or auth). Once action is processed, boolean status with colection of error messages will be returned.
`IMediator.Execute` - Executes Request from which you expects response with data. Response is wrapped to provide execution final status, error messages and all results from handler.

## Pipelines
TODO

### Miltiple handlers per action contract
TODO

### Create custom action types
TODO

### Error Handling
Mediator catches all exception which occures during midleware or action handler execution. If you want to provide Logging, please write your own logger like example below and put it to begining of your all mediator pipelines
```
public abstract class ExecutionMiddleware : IMediatorMiddleware
{
    public ILogger<ExecutionMiddleware> _logger;

    public ExecutionMiddleware(ILogger<ExecutionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
    {
        try
        {
            await next(context);
        }
        catch(Exception e)
        {
            _logger.LogError(e);
        }
    }    
}
```

#### !!! Important note !!!
Keep in mind that mediator handles all unhandled exceptions internally. That means that your ASP.NET Core middleware handling exceptions for example from ASP.NET Core MVC wont receive these exceptions.