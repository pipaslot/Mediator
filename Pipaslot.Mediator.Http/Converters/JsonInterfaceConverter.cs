﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Converters
{
    internal class JsonInterfaceConverter<T> : JsonConverter<T> where T : class
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Utf8JsonReader readerClone = reader;
            if (readerClone.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = readerClone.GetString();
            if (propertyName != "$type")
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            string typeValue = readerClone.GetString();
            var instance = Activator.CreateInstance(Type.GetType(typeValue));
            var entityType = instance.GetType();

            var deserialized = JsonSerializer.Deserialize(ref reader, entityType, options);
            return (T)deserialized;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (T)null, options);
                    break;
                default:
                    {
                        var type = value.GetType();
                        using var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(value, type, options));
                        writer.WriteStartObject();
                        writer.WriteString("$type", type.AssemblyQualifiedName);

                        foreach (var element in jsonDocument.RootElement.EnumerateObject())
                        {
                            element.WriteTo(writer);
                        }

                        writer.WriteEndObject();
                        break;
                    }
            }
        }
    }
}