using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json;

/// <summary>
///     Adds support for <see cref="DataContractAttribute" />, <see cref="DataMemberAttribute" /> and <see cref="IgnoreDataMemberAttribute"/> to STJ.
/// </summary>
internal sealed class NitroxJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    /// <summary>
    ///     Modifies the provided <paramref name="jsonTypeInfo" /> by replacing its properties
    ///     with those derived from <see cref="DataMemberAttribute" /> or
    ///     <see cref="IgnoreDataMemberAttribute" /> annotations.
    /// </summary>
    /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo" /> to modify.</param>
    /// <returns>The modified <paramref name="jsonTypeInfo" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="jsonTypeInfo" /> is <see langword="null" />.
    /// </exception>
    public static JsonTypeInfo GetTypeInfo(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo is null)
        {
            throw new ArgumentNullException(nameof(jsonTypeInfo));
        }

        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (JsonPropertyInfo? jsonPropertyInfo in CreateDataMembers(jsonTypeInfo).OrderBy(x => x.Order))
            {
                jsonTypeInfo.Properties.Add(jsonPropertyInfo);
            }
        }

        return jsonTypeInfo;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="type" /> or <paramref name="options" /> is <see langword="null" />.
    /// </exception>
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return jsonTypeInfo;
        }

        jsonTypeInfo.Properties.Clear();

        return GetTypeInfo(jsonTypeInfo);
    }

    private static bool IsNullOrDefault(object? obj)
    {
        if (obj is null)
        {
            return true;
        }

        Type type = obj.GetType();

        if (!type.IsValueType)
        {
            return false;
        }

        object? defaultValue = Activator.CreateInstance(type);
        return defaultValue is null || defaultValue.Equals(obj);
    }

    private static IEnumerable<MemberInfo> EnumerateFieldsAndProperties(Type type, BindingFlags bindingFlags)
    {
        foreach (FieldInfo fieldInfo in type.GetFields(bindingFlags))
        {
            yield return fieldInfo;
        }

        foreach (PropertyInfo propertyInfo in type.GetProperties(bindingFlags))
        {
            yield return propertyInfo;
        }
    }

    private static IEnumerable<JsonPropertyInfo> CreateDataMembers(JsonTypeInfo jsonTypeInfo)
    {
        bool isDataContract = jsonTypeInfo.Type.GetCustomAttribute<DataContractAttribute>() != null;
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;

        if (isDataContract)
        {
            bindingFlags |= BindingFlags.NonPublic;
        }

        foreach (MemberInfo memberInfo in EnumerateFieldsAndProperties(jsonTypeInfo.Type, bindingFlags))
        {
            DataMemberAttribute? attr = null;
            if (isDataContract)
            {
                attr = memberInfo.GetCustomAttribute<DataMemberAttribute>();
                if (attr == null)
                {
                    continue;
                }
            }
            else
            {
                if (memberInfo.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                {
                    continue;
                }
            }

            Func<object, object?>? getValue = null;
            Action<object, object?>? setValue = null;
            Type? propertyType;
            string? propertyName;

            if (memberInfo.MemberType == MemberTypes.Field && memberInfo is FieldInfo fieldInfo)
            {
                propertyName = attr?.Name ?? fieldInfo.Name;
                propertyType = fieldInfo.FieldType;
                getValue = fieldInfo.GetValue;
                setValue = fieldInfo.SetValue;
            }
            else if (memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo propertyInfo)
            {
                propertyName = attr?.Name ?? propertyInfo.Name;
                propertyType = propertyInfo.PropertyType;
                if (propertyInfo.CanRead)
                {
                    getValue = propertyInfo.GetValue;
                }
                if (propertyInfo.CanWrite)
                {
                    setValue = propertyInfo.SetValue;
                }
            }
            else
            {
                continue;
            }

            JsonPropertyInfo jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(propertyType, propertyName);

            jsonPropertyInfo.Get = getValue;
            jsonPropertyInfo.Set = setValue;

            if (attr != null)
            {
                jsonPropertyInfo.IsRequired = attr.IsRequired;
                jsonPropertyInfo.Order = attr.Order;
                jsonPropertyInfo.ShouldSerialize = !attr.EmitDefaultValue ? (_, obj) => !IsNullOrDefault(obj) : null;
            }

            if (!jsonPropertyInfo.IsRequired)
            {
                RequiredAttribute? requiredAttr = memberInfo.GetCustomAttribute<RequiredAttribute>();
                if (requiredAttr != null)
                {
                    jsonPropertyInfo.IsRequired = true;
                }
            }

            yield return jsonPropertyInfo;
        }
    }
}
