using BenchmarkDotNet.Attributes;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.V2;
using Pipaslot.Mediator.Http.Serialization.V3;
using System.Text.Json;

namespace Pipaslot.Mediator.Benchmarks;

public class SerializationBenchmark
{
    private readonly DataResult _result;
    private readonly JsonContractSerializer _v3Serializer;
    private readonly FullJsonContractSerializer _v2Serializer;
    private readonly JsonSerializerOptions _options;

    public SerializationBenchmark()
    {
        _result = new DataResult(5000);
        var credibleProviderMock = new FakeProvider();
        var options = new ClientMediatorOptions();
        _v3Serializer = new JsonContractSerializer(credibleProviderMock, options);
        _v2Serializer = new FullJsonContractSerializer(credibleProviderMock);
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
    public void V2Serializer()
    {
        var resp = new MediatorResponse(true, [_result]);
        var str = _v2Serializer.SerializeResponse(resp);
        _v2Serializer.DeserializeResponse<DataResult>(str);
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
        public DataResult()
        {
        }

        public DataResult(int rows)
        {
            Data = Enumerable.Range(1, rows).Select(i => new DataDto(i)).ToList();
        }

        public List<DataDto> Data { get; set; } = [];
    }

    private class DataDto
    {
        public string Prop1 { get; set; } = string.Empty;
        public string Prop2 { get; set; } = string.Empty;
        public string Prop3 { get; set; } = string.Empty;
        public string Prop4 { get; set; } = string.Empty;
        public string Prop5 { get; set; } = string.Empty;

        public DataDto() { }

        public DataDto(int index)
        {
            Prop1 = $"prop1-{index}";
            Prop2 = $"prop2-{index}";
            Prop3 = $"prop3-{index}";
            Prop4 = $"prop4-{index}";
            Prop5 = $"prop5-{index}";
        }
    }
}