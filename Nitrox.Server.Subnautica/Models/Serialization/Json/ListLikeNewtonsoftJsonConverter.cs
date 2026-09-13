using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json;

/// <summary>
///     Used to migrate away from NewtonsoftJSON style formats and output them as System.Text.Json format.
/// </summary>
// TODO: Stop using this when we moved all JSON on server to System.Text.Json native.
internal sealed class ListLikeNewtonsoftJsonConverter : JsonConverterFactory
{
    private const string LIST_ENTRY_JSON_PROPERTY_NAME = "value";

    private static readonly Type[] listInterfaces =
    [
        typeof(IList<>),
        typeof(IList),
        typeof(IReadOnlyList<>)
    ];

    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        return IsGenericListType(typeToConvert);

        static bool IsGenericListType(Type type)
        {
            foreach (Type? typeInterface in type.GetInterfaces())
            {
                foreach (Type? commonDictInterface in listInterfaces)
                {
                    if (commonDictInterface == typeInterface || (typeInterface.IsGenericType && commonDictInterface == typeInterface.GetGenericTypeDefinition()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public override JsonConverter CreateConverter(
        Type type,
        JsonSerializerOptions options)
    {
        Type[]? typeArguments = type.GetGenericArguments();
        Type? value = typeArguments[0];

        JsonConverter? converter = (JsonConverter)Activator.CreateInstance(
            typeof(ListConverterInner<>).MakeGenericType(value),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            [options],
            null)!;

        return converter;
    }

    private class ListConverterInner<TValue> : JsonConverter<IList<TValue>>
    {
        private readonly JsonConverter<TValue> valueConverter;
        private readonly Type valueType;

        public ListConverterInner(JsonSerializerOptions options)
        {
            valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));

            valueType = typeof(TValue);
        }

        public override IList<TValue> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            IList<TValue>? list = Activator.CreateInstance(typeToConvert) as IList<TValue> ?? throw new JsonException($"Type '{typeToConvert}' must be convertable to an IList<T>");

            if (!ReadAsNewtonsoftArray(ref reader, list, options))
            {
                ReadAsStjArray(ref reader, list, options);
            }
            return list;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IList<TValue> list,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (TValue? value in list)
            {
                valueConverter.Write(writer, value, options);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        ///     Read as NewtonsoftJSON formatted array.
        /// </summary>
        private bool ReadAsNewtonsoftArray(ref Utf8JsonReader reader, IList<TValue> list, JsonSerializerOptions options)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return true;
                }
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    continue;
                }
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    return false;
                }
                string? propertyName = reader.GetString();
                if (propertyName != LIST_ENTRY_JSON_PROPERTY_NAME)
                {
                    throw new JsonException();
                }

                // Get the value.
                reader.Read();
                TValue? value = valueConverter.Read(ref reader, valueType, options)!;

                list.Add(value);

                // End entry object.
                reader.Read();
            }
            return true;
        }

        /// <summary>
        ///     Read as System.Text.Json formatted array.
        /// </summary>
        private void ReadAsStjArray(ref Utf8JsonReader reader, IList<TValue> list, JsonSerializerOptions options)
        {
            // Skip read first time because it was done by ReadAsNewtonsoftArray.
            do
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return;
                }
                // Get the value.
                TValue? value = valueConverter.Read(ref reader, valueType, options)!;

                list.Add(value);
            } while (reader.Read());
        }
    }
}
