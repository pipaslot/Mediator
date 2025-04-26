using BenchmarkDotNet.Attributes;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.V3;
using System.Text.Json;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Compare Mediator serializer and the default .NET serializer
/// </summary>
[MemoryDiagnoser]
public class SerializationBenchmarks
{
    private DataResult _result = null!;
    private JsonContractSerializer _v3Serializer = null!;
    private JsonSerializerOptions _options = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _result = new DataResult
        {
            Data = Enumerable.Range(1, 1000).Select(index => new DataDto
            {
                Prop1 = $"prop1-{index}",
                Prop2 = $"prop2-{index}",
                Prop3 = $"prop3-{index}",
                Prop4 = $"prop4-{index}",
                Prop5 = $"prop5-{index}"
            }).ToList()
        };
        var credibleProviderMock = new FakeProvider();
        var options = new ClientMediatorOptions();
        _v3Serializer = new JsonContractSerializer(credibleProviderMock, options);
        _options = new JsonSerializerOptions { PropertyNamingPolicy = null };
    }

    [Benchmark]
    public void SystemTextJsonSerializer()
    {
        var resp = new FakeResponse { Success = true, Results = [_result] };
        var str = JsonSerializer.Serialize(resp, _options);
        JsonSerializer.Deserialize(str, typeof(FakeResponse), _options);
    }

    [Benchmark]
    public void V3Serializer()
    {
        var resp = new MediatorResponse(true, [_result]);
        var str = _v3Serializer.SerializeResponse(resp);
        _v3Serializer.DeserializeResponse<DataResult>(str);
    }

    private class FakeResponse
    {
        public bool Success { get; init; }
        public object[] Results { get; init; } = [];
    }

    private class FakeProvider : ICredibleProvider
    {
        public void VerifyCredibility(Type type)
        {
        }
    }

    private class DataResult
    {
        public List<DataDto> Data { get; init; } = [];
    }

    private class DataDto
    {
        public string Prop1 { get; init; } = string.Empty;
        public string Prop2 { get; init; } = string.Empty;
        public string Prop3 { get; init; } = string.Empty;
        public string Prop4 { get; init; } = string.Empty;
        public string Prop5 { get; init; } = string.Empty;
    }
}