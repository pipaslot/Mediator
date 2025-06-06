﻿using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.V3.Converters;

internal class InterfaceConverterFactory(ICredibleProvider credibleProvider) : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsInterface
               //Ignore collection interfaces
               && !typeof(IEnumerable).IsAssignableFrom(typeToConvert)
            ;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converter = (JsonConverter?)Activator.CreateInstance(
            typeof(InterfaceConverter<>).MakeGenericType(typeToConvert), credibleProvider);

        return converter ?? throw MediatorHttpException.CreateForNotConstructableJsonConverter();
    }
}