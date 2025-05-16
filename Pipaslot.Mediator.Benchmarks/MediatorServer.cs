using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Benchmarks.Actions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System.Net;
using System.Text;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Measure an overall mediator performance including HTTP request processing on the server side
/// </summary>
[MemoryDiagnoser]
public class MediatorServer
{
    private MediatorMiddleware _middleware = null!;
    private DefaultHttpContext _postMessageContext = null!;
    private DefaultHttpContext _postRequestContext = null!;
    private DefaultHttpContext _getMessageContext = null!;
    private DefaultHttpContext _getRequestContext = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddMediatorServer()
            .AddActions([typeof(MessageAction), typeof(RequestAction)])
            .AddHandlers([typeof(MessageActionHandler), typeof(RequestActionHandler)]);

        var serviceProvider = services.BuildServiceProvider();

        _middleware = new MediatorMiddleware(
            context => Task.CompletedTask, // next middleware
            new ServerMediatorOptions(),
            serviceProvider.GetRequiredService<IContractSerializer>(),
            serviceProvider.GetRequiredService<MediatorConfigurator>());

        _postMessageContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Request =
            {
                Method = "POST",
                Path = MediatorConstants.Endpoint,
                ContentType = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(
                    @"{ ""$type"":""Pipaslot.Mediator.Benchmarks.Actions.MessageAction, Pipaslot.Mediator.Benchmarks"" }"))
            }
        };

        _postRequestContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Request =
            {
                Method = "POST",
                Path = MediatorConstants.Endpoint,
                ContentType = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(
                    @"{ ""$type"":""Pipaslot.Mediator.Benchmarks.Actions.RequestAction, Pipaslot.Mediator.Benchmarks"", ""Message"":""Hello World"" }"))
            }
        };
        _getMessageContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Request =
            {
                Method = "GET",
                Path = MediatorConstants.Endpoint,
                QueryString = new QueryString(
                    $"?{MediatorConstants.ActionQueryParamName}={WebUtility.UrlDecode(@"{ ""$type"":""Pipaslot.Mediator.Benchmarks.Actions.MessageAction, Pipaslot.Mediator.Benchmarks"" }")}")
            }
        };

        _getRequestContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Request =
            {
                Method = "GET",
                Path = MediatorConstants.Endpoint,
                QueryString = new QueryString(
                    $"?{MediatorConstants.ActionQueryParamName}={WebUtility.UrlDecode(@"{ ""$type"":""Pipaslot.Mediator.Benchmarks.Actions.RequestAction, Pipaslot.Mediator.Benchmarks"", ""Message"":""Hello World"" }")}"),
            }
        };
    }

    [Benchmark]
    public async Task PostMessage()
    {
        await _middleware.Invoke(_postMessageContext);
    }

    [Benchmark]
    public async Task PostRequest()
    {
        await _middleware.Invoke(_postRequestContext);
    }

    [Benchmark]
    public async Task GetMessage()
    {
        await _middleware.Invoke(_getMessageContext);
    }

    [Benchmark]
    public async Task GetRequest()
    {
        await _middleware.Invoke(_getRequestContext);
    }
}