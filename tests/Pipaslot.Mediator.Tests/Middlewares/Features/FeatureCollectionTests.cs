using Pipaslot.Mediator.Middlewares.Features;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Tests.Middlewares.Features;

public class FeatureCollectionTests
{
    [Fact]
    public void Constructor_NegativeInitialCapacityThrows()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new FeatureCollection(-1));

        Assert.Equal("initialCapacity", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Constructor_NonNegativeInitialCapacityAllowsAddingItems(int capacity)
    {
        var interfaces = new FeatureCollection(capacity);
        var thing = new Thing();

        interfaces[typeof(IThing)] = thing;

        Assert.Equal(thing, interfaces[typeof(IThing)]);
    }

    [Fact]
    public void IndexerGet_WithNullKeyThrows()
    {
        var interfaces = new FeatureCollection();

        Assert.Throws<ArgumentNullException>(() => interfaces[null!]);
    }

    [Fact]
    public void IndexerSet_WithNullKeyThrows()
    {
        var interfaces = new FeatureCollection();

        Assert.Throws<ArgumentNullException>(() => interfaces[null!] = new Thing());
    }

    [Fact]
    public void IndexerGet_ReturnsNullWhenNotSetAndNoDefaults()
    {
        var interfaces = new FeatureCollection();

        Assert.Null(interfaces[typeof(IThing)]);
    }

    [Fact]
    public void IndexerGet_ReturnsDefaultsValueWhenNotSetLocally()
    {
        var defaults = new FeatureCollection();
        var thing = new Thing();
        defaults[typeof(IThing)] = thing;

        var interfaces = new FeatureCollection(defaults);

        Assert.Equal(thing, interfaces[typeof(IThing)]);
    }

    [Fact]
    public void IndexerSet_NullOnMissingKeyDoesNotIncrementRevision()
    {
        var interfaces = new FeatureCollection();

        interfaces[typeof(IThing)] = null;

        Assert.Equal(0, interfaces.Revision);
    }

    [Fact]
    public void Revision_StartsAtZero()
    {
        var interfaces = new FeatureCollection();

        Assert.Equal(0, interfaces.Revision);
    }

    [Fact]
    public void Revision_IncrementsOnSet()
    {
        var interfaces = new FeatureCollection();

        interfaces[typeof(IThing)] = new Thing();

        Assert.Equal(1, interfaces.Revision);
    }

    [Fact]
    public void Revision_IncrementsOnRemove()
    {
        var interfaces = new FeatureCollection();
        interfaces[typeof(IThing)] = new Thing();

        interfaces[typeof(IThing)] = null;

        Assert.Equal(2, interfaces.Revision);
    }

    [Fact]
    public void Revision_IncludesDefaultsRevision()
    {
        var defaults = new FeatureCollection();
        defaults[typeof(IThing)] = new Thing(); // defaults.Revision == 1

        var interfaces = new FeatureCollection(defaults);
        interfaces[typeof(Thing)] = new Thing(); // local revision == 1

        Assert.Equal(2, interfaces.Revision);
    }

    [Fact]
    public void GetEnumerator_OnEmptyCollection_ReturnsNoItems()
    {
        var interfaces = new FeatureCollection();

        Assert.Empty(interfaces);
    }

    [Fact]
    public void GetEnumerator_ReturnsLocalFeatures()
    {
        var interfaces = new FeatureCollection();
        var thing = new Thing();
        interfaces[typeof(IThing)] = thing;

        var pair = Assert.Single(interfaces);

        Assert.Equal(typeof(IThing), pair.Key);
        Assert.Equal(thing, pair.Value);
    }

    [Fact]
    public void GetEnumerator_WhenNoLocalFeatures_ReturnsDefaultsFeatures()
    {
        var defaults = new FeatureCollection();
        var thing = new Thing();
        defaults[typeof(IThing)] = thing;
        var interfaces = new FeatureCollection(defaults);

        var pair = Assert.Single(interfaces);

        Assert.Equal(typeof(IThing), pair.Key);
        Assert.Equal(thing, pair.Value);
    }

    [Fact]
    public void GetEnumerator_ExcludesDefaultsFeatureMaskedByLocalFeature()
    {
        // Exercises the private KeyComparer used by Enumerable.Except to detect the shared key.
        var defaults = new FeatureCollection();
        defaults[typeof(IThing)] = new Thing();

        var interfaces = new FeatureCollection(defaults);
        var localThing = new Thing();
        interfaces[typeof(IThing)] = localThing;

        var pair = Assert.Single(interfaces);

        Assert.Equal(typeof(IThing), pair.Key);
        Assert.Equal(localThing, pair.Value);
    }

    [Fact]
    public void GetEnumerator_CombinesLocalAndUnmaskedDefaultsFeatures()
    {
        var defaults = new FeatureCollection();
        defaults[typeof(IThing)] = new Thing();
        defaults[typeof(OtherThing)] = new OtherThing();

        var interfaces = new FeatureCollection(defaults);
        interfaces[typeof(IThing)] = new Thing(); // masks IThing coming from defaults

        var result = interfaces.ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Key == typeof(IThing));
        Assert.Contains(result, p => p.Key == typeof(OtherThing));
    }

    [Fact]
    public void NonGenericGetEnumerator_ReturnsSameItemsAsGenericOne()
    {
        var interfaces = new FeatureCollection();
        var thing = new Thing();
        interfaces[typeof(IThing)] = thing;

        var enumerator = ((IEnumerable)interfaces).GetEnumerator();

        Assert.True(enumerator.MoveNext());
        var pair = Assert.IsType<KeyValuePair<Type, object>>(enumerator.Current);
        Assert.Equal(typeof(IThing), pair.Key);
        Assert.Equal(thing, pair.Value);
        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void IndexerSet_InterfaceIsReturned()
    {
        var interfaces = new FeatureCollection();
        var thing = new Thing();

        interfaces[typeof(IThing)] = thing;

        var thing2 = interfaces[typeof(IThing)];
        Assert.Equal(thing2, thing);
    }

    [Fact]
    public void Indexer_AlsoAddsItems()
    {
        var interfaces = new FeatureCollection();
        var thing = new Thing();
        interfaces[typeof(IThing)] = thing;

        Assert.Equal(interfaces[typeof(IThing)], thing);
    }

    [Fact]
    public void IndexerSet_Null_RemovesValue()
    {
        var interfaces = new FeatureCollection();
        var thing = new Thing();

        interfaces[typeof(IThing)] = thing;
        Assert.Equal(interfaces[typeof(IThing)], thing);

        interfaces[typeof(IThing)] = null;

        var thing2 = interfaces[typeof(IThing)];
        Assert.Null(thing2);
    }

    [Fact]
    public void Get_MissingStructFeature_Throws()
    {
        var interfaces = new FeatureCollection();

        // Regression test: Used to throw NullReferenceException because it tried to unbox a null object to a struct
        var ex = Assert.Throws<InvalidOperationException>(() => interfaces.Get<int>());
        Assert.Equal(
            "System.Int32 does not exist in the feature collection and because it is a struct the method can't return null. Use 'featureCollection[typeof(System.Int32)] is not null' to check if the feature exists.",
            ex.Message);
    }

    [Fact]
    public void Get_MissingFeature_ReturnsNull()
    {
        var interfaces = new FeatureCollection();

        Assert.Null(interfaces.Get<Thing>());
    }

    [Fact]
    public void Get_StructFeature()
    {
        var interfaces = new FeatureCollection();
        var value = 20;
        interfaces.Set(value);

        Assert.Equal(value, interfaces.Get<int>());
    }

    [Fact]
    public void Get_NullableStructFeatureWhenSetWithNonNullableStruct()
    {
        var interfaces = new FeatureCollection();
        var value = 20;
        interfaces.Set(value);

        Assert.Null(interfaces.Get<int?>());
    }

    [Fact]
    public void Get_NullableStructFeatureWhenSetWithNullableStruct()
    {
        var interfaces = new FeatureCollection();
        var value = 20;
        interfaces.Set<int?>(value);

        Assert.Equal(value, interfaces.Get<int?>());
    }

    [Fact]
    public void Get_Feature()
    {
        var interfaces = new FeatureCollection();
        var thing = new Thing();
        interfaces.Set(thing);

        Assert.Equal(thing, interfaces.Get<Thing>());
    }

    public interface IThing
    {
        string Hello();
    }

    public class Thing : IThing
    {
        public string Hello()
        {
            return "World";
        }
    }

    public class OtherThing
    {
    }
}