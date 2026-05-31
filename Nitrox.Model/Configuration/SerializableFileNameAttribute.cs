using System;
using System.Reflection;

namespace Nitrox.Model.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public class SerializableFileNameAttribute : Attribute
{
    public SerializableFileNameAttribute(string fileName)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
    }

    public string FileName { get; init; }

    public static string GetFileName<T>() => typeof(T).GetCustomAttribute<SerializableFileNameAttribute>()?.FileName ?? throw new InvalidOperationException();
}
