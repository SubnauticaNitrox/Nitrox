using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class ReferenceHolder : MonoBehaviour
{
    private readonly Dictionary<Type, object> references = [];

    public bool TryGetReference<T>(out T outReference)
    {
        if (references.TryGetValue(typeof(T), out object value) && value is T reference)
        {
            outReference = reference;
            return true;
        }

        outReference = default;
        return false;
    }

    public void AddReference<T>(T reference)
    {
        references[typeof(T)] = reference;
    }
}
