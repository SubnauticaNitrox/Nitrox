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
internal sealed class DictionaryLikeNewtonsoftJsonConverter : JsonConverterFactory
{
    private static readonly Type[] dictionaryInterfaces =
    [
        typeof(IDictionary<,>),
        typeof(IDictionary),
        typeof(IReadOnlyDictionary<,>)
    ];

    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        return IsGenericDictionaryType(typeToConvert);

        static bool IsGenericDictionaryType(Type type)
        {
            foreach (Type? typeInterface in type.GetInterfaces())
            {
                foreach (Type? commonDictInterface in dictionaryInterfaces)
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
        Type? keyType = typeArguments[0];
        Type? valueType = typeArguments[1];

        JsonConverter? converter = (JsonConverter)Activator.CreateInstance(
            typeof(DictionaryConverterInner<,>).MakeGenericType(keyType, valueType),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            [options],
            null)!;

        return converter;
    }

    private class DictionaryConverterInner<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>> where TKey : notnull
    {
        private readonly JsonConverter<TKey> keyConverter;
        private readonly Type keyType;
        private readonly JsonConverter<TValue> valueConverter;
        private readonly Type valueType;

        public DictionaryConverterInner(JsonSerializerOptions options)
        {
            // For performance, use the existing converter.
            keyConverter = (JsonConverter<TKey>)options.GetConverter(typeof(TKey));
            valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));

            keyType = typeof(TKey);
            valueType = typeof(TValue);
        }

        public override IDictionary<TKey, TValue> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            IDictionary<TKey, TValue> dictionary = Activator.CreateInstance(typeToConvert) as IDictionary<TKey, TValue> ?? throw new JsonException($"Type '{typeToConvert}' must be convertable to an IDictionary<TKey, TValue>");

            if (!ReadAsNewtonsoftDictionary(ref reader, dictionary, options))
            {
                ReadAsStjDictionary(ref reader, dictionary, options);
            }

            return dictionary;
        }

        private bool ReadAsNewtonsoftDictionary(ref Utf8JsonReader reader, IDictionary<TKey, TValue> dictionary, JsonSerializerOptions options)
        {
            // Check if this looks like a Newtonsoft dictionary, which is an array.
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                return false;
            }

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

                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }
                reader.Read();
                TKey? key = keyConverter.Read(ref reader, keyType, options);
                if (key == null)
                {
                    throw new JsonException("Can not deserialize a dictionary with a null key");
                }

                // Get the value.
                reader.Read();
                reader.Read();
                TValue? value = valueConverter.Read(ref reader, valueType, options)!;
                reader.Read();
                if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Expected end of dictionary entry");
                }

                // Add to dictionary.
                dictionary.Add(key, value);
            }
            return true;
        }

        private void ReadAsStjDictionary(ref Utf8JsonReader reader, IDictionary<TKey, TValue> dictionary, JsonSerializerOptions options)
        {
            do
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return;
                }
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    continue;
                }

                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }
                TKey key = keyConverter.Read(ref reader, keyType, options);

                // Get the value.
                reader.Read();
                TValue value = valueConverter.Read(ref reader, valueType, options);

                // Add to dictionary.
                dictionary.Add(key, value);
            } while (reader.Read());
        }

        public override void Write(
            Utf8JsonWriter writer,
            IDictionary<TKey, TValue> dictionary,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach ((TKey? key, TValue? value) in dictionary)
            {
                string propertyName = key?.ToString() ?? throw new JsonException("Key of dictionary entry must not be null");
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);

                valueConverter.Write(writer, value, options);
            }

            writer.WriteEndObject();
        }
    }
}
