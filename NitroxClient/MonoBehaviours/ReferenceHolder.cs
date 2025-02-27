using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class ReferenceHolder : MonoBehaviour
{
    public object Reference;

    public bool TryGetReference<T>(out T outReference)
    {
        if (Reference is T reference)
        {
            outReference = reference;
            return true;
        }

        outReference = default;
        return false;
    }

    public static ReferenceHolder EnsureReferenceAttached(Component component, object reference)
    {
        return EnsureReferenceAttached(component.gameObject, reference);
    }

    public static ReferenceHolder EnsureReferenceAttached(GameObject gameObject, object reference)
    {
        ReferenceHolder referenceHolder = gameObject.EnsureComponent<ReferenceHolder>();
        referenceHolder.Reference = reference;
        return referenceHolder;
    }
}
