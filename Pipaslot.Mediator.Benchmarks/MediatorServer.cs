using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Benchmarks.Actions;
using Pipaslot.Mediator.Http;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System.Text;

namespace Pipaslot.Mediator.Benchmarks;
/// <summary>
/// Measure an overall mediator performance including HTTP request processing on the server side
/// </summary>
[MemoryDiagnoser]
public class MediatorServer
{
    private MediatorMiddleware _middleware = null!;
    private DefaultHttpContext _messageContext = null!;
    private DefaultHttpContext _requestContext = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddMediatorServer()
            .AddActions([typeof(MessageAction)])
            .AddHandlers([typeof(MessageActionHandler)]);

        var serviceProvider = services.BuildServiceProvider();

        _middleware = new MediatorMiddleware(
            context => Task.CompletedTask, // next middleware
            new ServerMediatorOptions(),
            serviceProvider.GetRequiredService<IContractSerializer>());

        _messageContext = new DefaultHttpContext
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
        
        _requestContext = new DefaultHttpContext
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
    }

    [Benchmark]
    public async Task Message()
    {
        await _middleware.Invoke(_messageContext);
    }

    [Benchmark]
    public async Task Request()
    {
        await _middleware.Invoke(_requestContext);
    }
}