using Demo.Server.Handlers;
using Demo.Server.MediatorMiddlewares;
using Demo.Shared;
using Demo.Shared.Requests;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Http;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllersWithViews();
services.AddRazorPages();
services.AddResponseCompression();
services.AddHttpContextAccessor();

//////// Mediator implementation
services.AddMediatorServer(o =>
{
    o.Endpoint = Constants.CustomMediatorUrl;
    o.ErrorHttpStatusCode = 500;
})
    .AddActionsFromAssemblyOf<WeatherForecast.Request>()
    .AddHandlersFromAssemblyOf<WheatherForecastRequestHandler>()
    .UseExceptionLogging()                  // Log all unhalded exception via ILogger
    // Configure pipelines for own custom action types. This is CQRS implementaiton Demo 
    //.MapWhen<IQuery>(s => s               // Pipeline specified only for queries
    //    .Use<QuerySpecificMiddleware>()   // Middleare which should be applied only to Queries
    //    )
    //.MapWhen<ICommand>(s => s             // Pipeline specified only for commands
    //    .Use<CommandSpecificMiddleware>() // Middleare which should be applied only to Commands
    //    )
    // Use default pipeline if you do not use Action specific specific middlewares or any from previous pipelines does not fullfil condition for execution   
    .Use<CallStackLoggerMiddleware>()
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