using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Benchmarks.Actions;
using Pipaslot.Mediator.Http;
using System.Text.Json;
using IHost = Microsoft.Extensions.Hosting.IHost;


[MemoryDiagnoser]
public class MediatorVsAspNet
{
    private IHost _host;
    private HttpClient _client;
    private StringContent _aspnetContent = new(@"{}");

    private StringContent _mediatorContent =
        new(@"{ ""$type"":""Pipaslot.Mediator.Benchmarks.Actions.MessageAction, Pipaslot.Mediator.Benchmarks"" }");

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging => logging.ClearProviders())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.Configure(app =>
                {
                    app.Map("/middleware", builder => builder.UseMiddleware<DummyMiddleware>());
                    app.UseMediator();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapPost("/minimal", async (IDummyService service) =>
                        {
                            await service.DoSomethingAsync();
                            return new MediatorResponse(true, []);
                        });

                        endpoints.MapControllers();
                    });
                });
            });
        builder.ConfigureServices(services =>
        {
            services.AddControllers();
            services.AddScoped<IDummyService, DummyService>();

            services.AddMediatorServer()
                .AddActions([typeof(MessageAction)])
                .AddHandlers([typeof(MessageActionHandler)]);
        });
        _host = builder.Build();

        await _host.StartAsync();
        _client = _host.GetTestClient();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    [Benchmark(Baseline = true)]
    public async Task Middleware()
    {
        var response = await _client.PostAsync("/middleware", _aspnetContent);
        response.EnsureSuccessStatusCode();
        _ = await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task MinimalApi()
    {
        var response = await _client.PostAsync("/minimal", _aspnetContent);
        response.EnsureSuccessStatusCode();
        _ = await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task Controller()
    {
        var response = await _client.PostAsync("/Dummy", _aspnetContent);
        response.EnsureSuccessStatusCode();
        _ = await response.Content.ReadAsStringAsync();
    }

    [Benchmark]
    public async Task Mediator()
    {
        var response = await _client.PostAsync(MediatorConstants.Endpoint, _mediatorContent);
        response.EnsureSuccessStatusCode();
        _ = await response.Content.ReadAsStringAsync();
    }
}

public interface IDummyService
{
    Task DoSomethingAsync();
}

public class DummyService : IDummyService
{
    public Task DoSomethingAsync() => Task.CompletedTask;
}

public class DummyMiddleware(RequestDelegate next, IDummyService service)
{
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments(""))
        {
            await service.DoSomethingAsync();
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new MediatorResponse(true, [])));
        }
        else
        {
            await next(context);
        }
    }
}

[ApiController]
[Route("[controller]")]
public class DummyController(IDummyService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post()
    {
        await service.DoSomethingAsync();
        return new JsonResult(new MediatorResponse(true, []));
    }
}