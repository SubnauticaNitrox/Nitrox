using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Unity.Helper
{
    public static class GameObjectHelper
    {
        public static T RequireComponent<T>(this GameObject o) where T : class
        {
            T component = o.GetComponent<T>();
            Validate.NotNull(component, o.name + " did not have a component of type " + typeof(T));

            return component;
        }

        public static T RequireComponentInChildren<T>(this GameObject o, bool includeInactive = false) where T : class
        {
            T component = o.GetComponentInChildren<T>(includeInactive);
            Validate.NotNull(component, o.name + " did not have a component of type " + typeof(T) + " in its children");

            return component;
        }

        public static T RequireComponentInParent<T>(this GameObject o) where T : class
        {
            T component = o.GetComponentInParent<T>();
            Validate.NotNull(component, o.name + " did not have a component of type " + typeof(T) + " in its parent");

            return component;
        }
    }
}
