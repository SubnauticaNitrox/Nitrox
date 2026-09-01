using System;
using System.Reflection;
using Nitrox.Model.Core;
using UnityEngine;

namespace NitroxClient.Extensions;

public static class UnityObjectExtensions
{
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
