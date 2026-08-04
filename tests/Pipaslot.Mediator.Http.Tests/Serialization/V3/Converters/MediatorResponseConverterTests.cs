using NSubstitute;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3.Converters;
using System;
using System.Linq;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3.Converters;

/// <summary>
/// Direct converter-level tests, complementing the integration-level coverage in
/// Serialization/V3/JsonContractSerializer_* and Serialization/ContractSerializer_*TestBase.
/// </summary>
public class MediatorResponseConverterTests
{
    private readonly ICredibleProvider _credibleProvider = Substitute.For<ICredibleProvider>();

    private JsonSerializerOptions CreateOptions(ICredibleProvider? credibleProvider = null)
    {
        var provider = credibleProvider ?? _credibleProvider;
        var options = new JsonSerializerOptions();
        options.Converters.Add(new MediatorResponseConverter(provider));
        options.Converters.Add(new InterfaceConverterFactory(provider));
        return options;
    }

    #region Response shape

    [Fact]
    public void Read_MissingResultsProperty_ReturnsEmptyResults()
    {
        var options = CreateOptions();

        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>("""{"Success":true}""", options)!;

        Assert.True(deserialized.Success);
        Assert.Empty(deserialized.Results);
    }

    [Fact]
    public void Read_MissingSuccessProperty_DefaultsToFalse()
    {
        var options = CreateOptions();

        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>("""{"Results":[]}""", options)!;

        Assert.False(deserialized.Success);
        Assert.Empty(deserialized.Results);
    }

    [Fact]
    public void Read_UnknownScalarProperty_IsIgnored()
    {
        // A forward-compatible payload may carry extra scalar properties (e.g. from a newer server version).
        var options = CreateOptions();

        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(
            """{"Success":true,"ServerVersion":"1.2.3","Results":[]}""", options)!;

        Assert.True(deserialized.Success);
        Assert.Empty(deserialized.Results);
    }

    [Fact]
    public void Read_UnknownObjectValuedProperty_IsSkippedEntirely()
    {
        // Regression test: an unknown property whose value is itself a JSON object (e.g. "Meta":{}) used to
        // make the outer loop's unscoped "if EndObject break" fire on the nested object's closing brace
        // instead of the response's own, which made System.Text.Json's VerifyRead check throw "read too much
        // or not enough". Fixed by reader.Skip()-ing unrecognized property values instead of leaving them
        // half-consumed.
        var options = CreateOptions();

        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(
            """{"Meta":{"RequestId":"abc","Nested":{"A":1}},"Success":true,"Results":[]}""", options)!;

        Assert.True(deserialized.Success);
        Assert.Empty(deserialized.Results);
    }

    [Fact]
    public void Read_UnknownArrayValuedProperty_IsSkippedEntirely()
    {
        var options = CreateOptions();

        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(
            """{"Warnings":[{"Code":1},{"Code":2}],"Success":true,"Results":[]}""", options)!;

        Assert.True(deserialized.Success);
        Assert.Empty(deserialized.Results);
    }

    #endregion

    #region Primitive results (Value property)

    [Fact]
    public void RoundTrip_EnumResult_PreservesValue()
    {
        // AsPrimitive() special-cases IsEnum; no existing test exercises an enum result at all.
        var options = CreateOptions();
        var response = new MediatorResponse(true, [Color.Green]);

        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);
        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(json, options)!;

        Assert.Equal(Color.Green, deserialized.Results[0]);
    }

    [Fact]
    public void RoundTrip_GuidResult_PreservesValue()
    {
        // AsPrimitive() special-cases Guid; no existing test exercises a Guid result at all.
        var options = CreateOptions();
        var value = Guid.NewGuid();
        var response = new MediatorResponse(true, [value]);

        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);
        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(json, options)!;

        Assert.Equal(value, deserialized.Results[0]);
    }

    [Fact]
    public void Write_PrimitiveResult_ProducesValuePropertyWithTypeIdentifier()
    {
        var options = CreateOptions();
        var response = new MediatorResponse(true, [42]);

        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);

        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(int)));
        var expected = $$"""{"Success":true,"Results":[{"$type":{{typeJson}},"Value":42}]}""";
        Assert.Equal(expected, json);
    }

    [Fact]
    public void RoundTrip_MultiplePrimitiveResults_DeserializesAllInOrder()
    {
        // Each primitive item leaves its wrapping object's closing brace for ReadResults' EndObject-skip
        // branch to consume; this was previously only exercised with a single result per response.
        var options = CreateOptions();
        var response = new MediatorResponse(true, [1, "two", true]);

        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);
        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(json, options)!;

        Assert.Equal(3, deserialized.Results.Length);
        Assert.Equal(1, deserialized.Results[0]);
        Assert.Equal("two", deserialized.Results[1]);
        Assert.Equal(true, deserialized.Results[2]);
    }

    #endregion

    #region Collection results (Items property)

    [Fact]
    public void RoundTrip_ArrayOfPrimitiveItems_Items()
    {
        var options = CreateOptions();
        var collection = new[] { 1, 2, 3 };
        var response = new MediatorResponse(true, [collection]);

        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);
        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(json, options)!;

        Assert.Equal(collection, (int[])deserialized.Results[0]);
    }

    [Fact]
    public void RoundTrip_EmptyCollection_Items()
    {
        var options = CreateOptions();
        var collection = Array.Empty<Contract>();
        var response = new MediatorResponse(true, [collection]);

        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);
        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(json, options)!;

        Assert.Empty((Contract[])deserialized.Results[0]);
    }

    [Fact]
    public void Write_CollectionResult_ProducesItemsPropertyWithTypeIdentifier()
    {
        var options = CreateOptions();
        var collection = new[] { new Contract { Name = "A" } };
        var response = new MediatorResponse(true, [collection]);

        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);

        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(Contract[])));
        var expected = $$"""{"Success":true,"Results":[{"$type":{{typeJson}},"Items":[{"Name":"A"}]}]}""";
        Assert.Equal(expected, json);
    }

    [Fact]
    public void Read_CollectionOfNonInterfaceItems_VerifiesCredibilityOfArrayType()
    {
        var credibleProvider = Substitute.For<ICredibleProvider>();
        var options = CreateOptions(credibleProvider);
        var collection = new[] { new Contract { Name = "A" } };
        var response = new MediatorResponse(true, [collection]);
        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);

        JsonSerializer.Deserialize<IMediatorResponse>(json, options);

        credibleProvider.Received(1).VerifyCredibility(typeof(Contract[]));
        Assert.Single(credibleProvider.ReceivedCalls());
    }

    [Fact]
    public void Read_CollectionOfInterfaceItems_SkipsCredibilityVerificationOfArrayType()
    {
        // Ignored for the array type itself because an interface array has $type specified per member,
        // and per-item credibility is verified by the nested InterfaceConverter instead.
        var credibleProvider = Substitute.For<ICredibleProvider>();
        var options = CreateOptions(credibleProvider);
        IContract[] collection = [new Contract { Name = "A" }];
        var response = new MediatorResponse(true, [collection]);
        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);

        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(json, options)!;

        credibleProvider.Received(0).VerifyCredibility(typeof(IContract[]));
        credibleProvider.Received(1).VerifyCredibility(typeof(Contract));
        Assert.Equal("A", ((Contract)((IContract[])deserialized.Results[0])[0]).Name);
    }

    [Fact]
    public void Read_ArrayResultMissingItemsProperty_ThrowsJsonException()
    {
        var options = CreateOptions();
        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(Contract[])));
        var json = $$"""{"Success":true,"Results":[{"$type":{{typeJson}}}]}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IMediatorResponse>(json, options));
    }

    [Fact]
    public void Read_ArrayResultWrongItemsPropertyName_ThrowsJsonException()
    {
        var options = CreateOptions();
        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(Contract[])));
        var json = $$"""{"Success":true,"Results":[{"$type":{{typeJson}},"Foo":[]}]}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IMediatorResponse>(json, options));
    }

    #endregion

    #region Mixed result kinds in a single response

    [Fact]
    public void RoundTrip_MixedResultKinds_PrimitiveThenCollectionThenObject_DeserializesAllInOrder()
    {
        // Primitive/collection items leave their wrapper's closing brace for the outer loop to skip,
        // while a plain object item consumes its own closing brace directly - never tested back-to-back
        // in the same Results array, so a regression in reader-position handling for one kind could
        // silently corrupt parsing of the next.
        var options = CreateOptions();
        var collection = new[] { new Contract { Name = "A" }, new Contract { Name = "B" } };
        var obj = new Contract { Name = "C" };
        var response = new MediatorResponse(true, [42, collection, obj]);

        var json = JsonSerializer.Serialize<IMediatorResponse>(response, options);
        var deserialized = JsonSerializer.Deserialize<IMediatorResponse>(json, options)!;

        Assert.Equal(3, deserialized.Results.Length);
        Assert.Equal(42, deserialized.Results[0]);
        Assert.Equal(["A", "B"], ((Contract[])deserialized.Results[1]).Select(c => c.Name));
        Assert.Equal("C", ((Contract)deserialized.Results[2]).Name);
    }

    #endregion

    public enum Color
    {
        Red,
        Green,
        Blue
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
