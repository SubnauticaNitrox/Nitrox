using System.Linq;
using System.Reflection;
using Nitrox.Server.Subnautica.Models.Attributes;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

/// <summary>
///     Defines a per-player property that will be persisted by the server.
/// </summary>
/// <typeparam name="T">The <see cref="Type" /> of the property.</typeparam>
internal interface IPlayerProperty<T> : IPlayerProperty
{
    new T Value { get; set; }

    object IPlayerProperty.Value
    {
        get => Value;
        set => Value = (T)value;
    }
}

internal interface IPlayerProperty
{
    object? Value { get; set; }

    string GetName()
    {
        Type type = GetType();
        return type.GetCustomAttribute<NameAttribute>()?.Name ?? type.Name.Replace("Property", "");
    }

    string GetGroupName()
    {
        Type type = GetType();
        return type.GetCustomAttribute<GroupAttribute>()?.Name ?? "";
    }

    Type GetTypeOfValue() => GetType().GetInterfaces().First().GetGenericArguments().First();
}
