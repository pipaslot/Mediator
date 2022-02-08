using Pipaslot.Mediator.Http;
using Demo.Server.Handlers;
using Demo.Server.MediatorMiddlewares;
using Demo.Shared;
using Demo.Shared.Requests;
using Pipaslot.Mediator.Notifications;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllersWithViews();
services.AddRazorPages();
services.AddResponseCompression();
services.AddHttpContextAccessor();

//////// Mediator implementation
services.AddMediatorServer(o => {
    o.Endpoint = Constants.CustomMediatorUrl;
})
    .AddActionsFromAssemblyOf<WeatherForecast.Request>()
    .AddHandlersFromAssemblyOf<WheatherForecastRequestHandler>()
    // Configure pipelines for own custom action types. This is CQRS implementaiton Demo 
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
        .Use<MediatorCallStackLoggerMiddleware>()
        .Use<NotificationSenderMiddleware>()
        .Use<ValidatorMiddleware>()
        .Use<CommonMiddleware>();
////////

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}
app.UseResponseCompression();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
//////// Mediator implementation
app.UseMediator(app.Environment.IsDevelopment());
////////
app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();