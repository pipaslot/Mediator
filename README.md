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
    private WeatherForecast.Result[] _forecasts;
    protected override async Task OnInitializedAsync()
    {
        var result = await Mediator.Execute(new WeatherForecast.Request());
        if (result.Success)
        {
            _forecasts = result.Result;
            ...
        }
    }

}
```
Or if you prefer direct access to result and exception are thrown in case of error:
```
@inject IMediator Mediator

@code {
    private WeatherForecast.Result[] _forecasts;
    protected override async Task OnInitializedAsync()
    {
        _forecasts = await Mediator.ExecuteUnhandled(new WeatherForecast.Request());
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
        services.AddMediatorServer()
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

These two methods do not return data directly, but they wrapps the data into `MediatorResponse`. This wrapper holds the handler result (or more objects if you have multiple handlers configured), status flag if the operation failed or succeeded and error messages from exceptions caught during processing.

You can also use alternative methods `IMediator.DispatchUnhandled` and `IMediator.ExecuteUnhandled` which returns result object directly. In case of error, exception will be thrown.

### Passing data types across network
Data sent through the network are serialized and deserialized into the same object type. This keeps you a possibility to use type checking `mediatorResponse.Result is MyCustomHandlerResult` or to use property attributes for a custom purposes like shared validation rules on client and server.

The dark side of this fact is, that **types that are unknown or unavailable on client won't be deserialized** and exceptions will be thrown. 


## Pipelines
Pipelines are optional. The purpose is to provide different processing for different action types. For example you want to apply caching for Request reponses which should not affect messages. 
From oposite site you want to audit all messages sent throug mediator but do not want to audit Requests. To gain the expected result we will define two pipelines.
``` .AddMediatorServer()
    ... //Action and handler registrations
    .AddPipeline<IMessage>()
        .Use<AuditMessageMediatorMiddleware()
        .Use<AnotherMessageSpecificMediatorMiddleware>()
    .AddPipeline<IRequest>()
        .Use<CacheRequestResponseMediatorMiddleware()
```
For more complex sample we may decide to audit only some specific Messages which has interface `IAuditableMessage` inheriting from`IMessage`. In this case we would update the mediator configuration to:
``` .AddMediatorServer()
    ... //Action and handler registrations
    .AddPipeline<IAuditableMessage>()
        .Use<AuditMessageMediatorMiddleware()
        .Use<AnotherMessageSpecificMediatorMiddleware>()
    .AddPipeline<IMessage>()
        .Use<AnotherMessageSpecificMediatorMiddleware>()
    .AddPipeline<IRequest>()
        .Use<CacheRequestResponseMediatorMiddleware()
```
Pay attention on the ordering in pipeline definitions! `IAuditableMessage` is more specific type. If we would register `AddPipeline<IMessage>()` before `AddPipeline<IAuditableMessage>()`, pipeline for `IAuditableMessage` would never been used!

### Multiple handlers per action contract
Sometimes multiple actions are expected to be executed. For example, you would like to forward the message to another audit server meanwhile the message is processed on your server. 
Or you would like to chain another action-handler once the first was finished.

For this purpose you can register middlewarer in mediator with `UseConcurrentMultiHandler()` and `UseSequenceMultiHandler()`
``` .AddMediatorServer()
    ... //Action and handler registrations
    .AddPipeline<...>()
        .UseConcurrentMultiHandler()
    .AddPipeline<...>() 
        .UseSequenceMultiHandler()
```

If you need to define the order of how the sequence handlers will be executed, then implement interface `ISequenceHandler` in your handlers and define ordering.


### Create custom action types
Don't you like naming as Request and Message? Or do you want to provide more action types to cover your specific pipeline behaviour? Just defino own Action and message types.
Here is example of naming for CQRS (Command and Query Responsibility Segregation)

ICommand.cs
```
using Pipaslot.Mediator;

namespace CustomMediator.Cqrs
{
    /// <summary>
    /// Marker type for actions and pipelines
    /// </summary>
    public interface ICommand :  : IMediatorAction
    {
    }
}
```

ICommandHandler.cs
```
using Pipaslot.Mediator.Abstraction;

namespace CustomMediator.Cqrs
{
    public interface ICommandHandler<TCommand> : IMediatorHandler<TCommand> where TCommand : ICommand
    {

    }
}
```

IQuery.cs
```
using Pipaslot.Mediator;

namespace CustomMediator.Cqrs
{
    /// <summary>
    /// Marker type for actions
    /// </summary>
    public interface IQuery<TResult> : IQuery, IMediatorAction<TResult>
    {
    }
    /// <summary>
    /// Marker type for pipelines
    /// </summary>
    public interface IQuery : IMediatorAction
    {
    }
}
```

IQueryHandler.cs
```
using Pipaslot.Mediator.Abstraction;

namespace CustomMediator.Cqrs
{
    public interface IQueryHandler<TQuery, TResult> : IMediatorHandler<TQuery, TResult> where TQuery: IQuery<TResult>
    {

    }
}

```

And that is it. Easy!

## Error Handling
Mediator catches all exception which occures during midleware or action handler execution. If you want to provide Logging via ILogger interface, you can create own logging middleware or register already prepared middleware in your pipelines by `.UseExceptionLogging()`
Keep in mind that mediator handles all unhandled exceptions internally. That means that your ASP.NET Core middleware handling exceptions for example from ASP.NET Core MVC wont receive these exceptions.
