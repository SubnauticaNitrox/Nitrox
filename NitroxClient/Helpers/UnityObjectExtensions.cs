using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.Helpers;

public static class UnityObjectExtensions
{
    /// <summary>
    ///     Resolves a type using <see cref="NitroxServiceLocator.LocateService{T}" />. If the result is not null it will cache and return the same type on future calls.
    /// </summary>
    /// <remarks>
    ///     Dependency Injection should be limited to UnityEngine object types as in other cases it should be injected as constructor parameter.
    ///     This is the reason for having UnityEngine.Object as first parameter.
    /// </remarks>
    /// <typeparam name="T">Type to get and cache from <see cref="NitroxServiceLocator" /></typeparam>
    /// <returns>The requested type or null if not available.</returns>
    public static T Resolve<T>(this UnityEngine.Object _, bool prelifeTime = false) where T : class
    {
        return prelifeTime ? NitroxServiceLocator.Cache<T>.ValuePreLifetime : NitroxServiceLocator.Cache<T>.Value;
    }

    /// <summary>
    /// Copies a whole component by using reflection. Please note this takes considerable time and every use of this should be thoughtful.
    /// </summary>
    public static Component CopyComponent(this Component original, GameObject destination)
    {
        Type type = original.GetType();
        Component copy = destination.AddComponent(type);

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            if (property.GetSetMethod(true) != null)
            {
                property.SetValue(copy, property.GetValue(original));
            }
        }
        return copy;
    }

    public static bool TryFind(string name, out GameObject gameObject)
    {
        gameObject = GameObject.Find(name);
        return gameObject;
    }
}
