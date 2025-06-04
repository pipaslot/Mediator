using Demo.Server.Handlers;
using Demo.Server.Handlers.Auth;
using Demo.Server.MediatorMiddlewares;
using Demo.Shared;
using Demo.Shared.Playground;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddRazorPages();
services.AddResponseCompression();
services.AddHttpContextAccessor();

//JWT
var authSection = builder.Configuration.GetSection("Auth");
services.Configure<LoginRequestHandler.AuthOptions>(authSection);
var authOptions = new LoginRequestHandler.AuthOptions();
authSection.Bind(authOptions);
//Configure JWT claim binding to Identity claims 
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap["role"] = ClaimTypes.Role;
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authOptions.Issuer,
                ValidAudience = authOptions.Audience,
                IssuerSigningKey = LoginRequestHandler.CreateSymetricKey(authOptions.SecretKey)
            };
    });

//////// Mediator implementation
services.AddMediatorServer(o =>
    {
        o.Endpoint = Constants.CustomMediatorUrl;
        o.IgnoreReadOnlyProperties = true;
    })
    .AddActionsFromAssemblyOf<WeatherForecast.Request>()
    .AddHandlersFromAssemblyOf<WeatherForecastRequestHandler>()
    // Log all unhalded exception via ILogger. Wont catch exception from IMessage as the next middleware provides custom handling for the Messages
    .UseExceptionLogging()
    .UseWhenAction<CustomInternalRequest>(s=>s.UseDirectHttpCallProtection())
    .UseWhenAction<IMessage>(
        p => p.Use<CustomLoggingMiddleware>()
    )
    // Configure pipelines for own custom action types. This is CQRS implementaiton Demo 
    //.UseWhen<IQuery>(s => s               // Pipeline specified only for queries
    //    .Use<QuerySpecificMiddleware>()   // Middleare which should be applied only to Queries
    //    )
    //.UseWhen<ICommand>(s => s             // Pipeline specified only for commands
    //    .Use<CommandSpecificMiddleware>() // Middleare which should be applied only to Commands
    //    )
    // Use default pipeline if you do not use Action specific specific middlewares or any from previous pipelines does not fullfil condition for execution   
    .UseAuthorizationWhenDirectHttpCall()
    .Use<CallStackLoggerMiddleware>()
    .Use<ValidatorMiddleware>()
    .Use<CommonMiddleware>();
////////

var app = builder.Build();
var isDev = app.Environment.IsDevelopment();

if (isDev)
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
app.UseAuthentication();

//////// Mediator implementation
app.UseMediator(isDev, isDev);
////////
app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");
app.Run();