using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Helpers;

public static class GameObjectReferenceHolderExtensions
{
    public static ReferenceHolder AddReference<T>(this Component component, T reference)
    {
        return AddReference(component.gameObject, reference);
    }

    public static ReferenceHolder AddReference<T>(this GameObject gameObject, T reference)
    {
        ReferenceHolder referenceHolder = gameObject.EnsureComponent<ReferenceHolder>();
        referenceHolder.AddReference(reference);
        return referenceHolder;
    }

    public static bool TryGetReference<T>(this GameObject gameObject, out T reference)
    {
        if (gameObject.TryGetComponent(out ReferenceHolder holder))
        {
            return holder.TryGetReference(out reference);
        }

        reference = default;
        return false;
    }

    public static bool TryGetReference<T>(this Component component, out T reference)
    {
        if (component.TryGetComponent(out ReferenceHolder holder))
        {
            return holder.TryGetReference(out reference);
        }

        reference = default;
        return false;
    }
}
