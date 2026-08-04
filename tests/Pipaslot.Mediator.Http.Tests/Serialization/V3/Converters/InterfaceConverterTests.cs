using NSubstitute;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3.Converters;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3.Converters;

public class InterfaceConverterTests
{
    private readonly ICredibleProvider _credibleProvider = Substitute.For<ICredibleProvider>();

    // InterfaceConverterFactory.CanConvert excludes collection interfaces, so InterfaceConverter<T>
    // is never created by the factory for an enumerable T. Registering it directly here lets us reach
    // the array-handling branches, mirroring how JsonContractSerializer registers InterfaceConverter<IMediatorAction> directly.
    private JsonSerializerOptions CreateOptions<T>()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new InterfaceConverter<T>(_credibleProvider));
        options.Converters.Add(new InterfaceConverterFactory(_credibleProvider));
        return options;
    }

    [Fact]
    public void Read_ValueIsNotObject_ThrowsJsonException()
    {
        var options = CreateOptions<IContract>();

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IContract>("\"not-an-object\"", options));
    }

    [Fact]
    public void Read_TypeValueIsNotString_ThrowsJsonException()
    {
        var options = CreateOptions<IContract>();

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IContract>(@"{""$type"":123}", options));
    }

    [Fact]
    public void RoundTrip_ArrayOfNonInterfaceItems_VerifiesCredibilityOfArrayTypeAndDeserializesItems()
    {
        var options = CreateOptions<IEnumerable>();
        IEnumerable value = new Contract[] { new() { Name = "A" } };

        var json = JsonSerializer.Serialize(value, options);
        var deserialized = (Contract[])JsonSerializer.Deserialize<IEnumerable>(json, options)!;

        _credibleProvider.Received(1).VerifyCredibility(typeof(Contract[]));
        Assert.Equal("A", deserialized[0].Name);
    }

    [Fact]
    public void RoundTrip_ArrayOfInterfaceItems_SkipsCredibilityVerificationOfArrayType()
    {
        var options = CreateOptions<IEnumerable>();
        IEnumerable value = new IContract[] { new Contract { Name = "A" } };

        var json = JsonSerializer.Serialize(value, options);
        var deserialized = (IContract[])JsonSerializer.Deserialize<IEnumerable>(json, options)!;

        _credibleProvider.Received(0).VerifyCredibility(typeof(IContract[]));
        Assert.Equal("A", ((Contract)deserialized[0]).Name);
    }

    [Fact]
    public void RoundTrip_EmptyArrayOfInterfaceItems_Items()
    {
        var options = CreateOptions<IEnumerable>();
        IEnumerable value = Array.Empty<IContract>();

        var json = JsonSerializer.Serialize(value, options);
        var deserialized = (IContract[])JsonSerializer.Deserialize<IEnumerable>(json, options)!;

        Assert.Empty(deserialized);
    }

    [Fact]
    public void Write_ArrayOfInterfaceItems_ProducesItemsPropertyWithTypeIdentifier()
    {
        // Pairs with RoundTrip_ArrayOfInterfaceItems_SkipsCredibilityVerificationOfArrayType above,
        // which round-trips the same shape but never pins the exact wire format.
        var options = CreateOptions<IEnumerable>();
        IEnumerable value = new IContract[] { new Contract { Name = "A" } };

        var json = JsonSerializer.Serialize(value, options);

        var arrayTypeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(IContract[])));
        var itemTypeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(Contract)));
        var expected = $$"""{"$type":{{arrayTypeJson}},"Items":[{"$type":{{itemTypeJson}},"Name":"A"}]}""";
        Assert.Equal(expected, json);
    }

    [Fact]
    public void RoundTrip_NonArrayObject_VerifiesCredibilityAndDeserializes()
    {
        // Exercises the "else" branch of Read (arrayItemType == null) directly at converter level;
        // previously only reached indirectly via JsonContractSerializer_InterfaceTests/CredibilityTests.
        var options = CreateOptions<IContract>();
        IContract value = new Contract { Name = "A" };

        var json = JsonSerializer.Serialize(value, options);
        var deserialized = JsonSerializer.Deserialize<IContract>(json, options)!;

        _credibleProvider.Received(1).VerifyCredibility(typeof(Contract));
        Assert.Equal("A", ((Contract)deserialized).Name);
    }

    [Fact]
    public void Read_ArrayResultMissingItemsProperty_ThrowsJsonException()
    {
        var options = CreateOptions<IEnumerable>();
        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(Contract[])));
        var json = $$"""{"$type":{{typeJson}}}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IEnumerable>(json, options));
    }

    [Fact]
    public void Read_ArrayResultWrongItemsPropertyName_ThrowsJsonException()
    {
        var options = CreateOptions<IEnumerable>();
        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(Contract[])));
        var json = $$"""{"$type":{{typeJson}},"Foo":[]}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IEnumerable>(json, options));
    }

    [Fact]
    public void Write_NullValue_WritesEmptyString()
    {
        // System.Text.Json intercepts null before invoking the converter for reference types (HandleNull defaults to false),
        // so this branch is otherwise unreachable through JsonSerializer.Serialize and must be called directly.
        var converter = new InterfaceConverter<IContract>(_credibleProvider);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            converter.Write(writer, null!, new JsonSerializerOptions());
        }

        Assert.Equal("\"\"", Encoding.UTF8.GetString(stream.ToArray()));
    }

    public interface IContract
    {
        string Name { get; set; }
    }

    public class Contract : IContract
    {
        public string Name { get; set; } = "";
    }
}
