using UnityEngine;

namespace NitroxModel.Helper.Unity
{
    public static class GameObjectHelper
    {
        public static T RequireComponent<T>(this GameObject o) where T : class
        {
            T component = o.GetComponent<T>();
            Validate.NotNull<T>(component, o.name + " did not have a component of type " + typeof(T));

            return component;
        }

        public static T RequireComponentInChildren<T>(this GameObject o) where T : class
        {
            T component = o.GetComponentInChildren<T>();
            Validate.NotNull<T>(component, o.name + " did not have a component of type " + typeof(T) + " in its children");

            return component;
        }

        public static T RequireComponentInChildren<T>(this GameObject o, bool includeInactive) where T : class
        {
            T component = o.GetComponentInChildren<T>(includeInactive);
            Validate.NotNull<T>(component, o.name + " did not have a component of type " + typeof(T) + " in its children");

            return component;
        }

        public static T RequireComponentInParent<T>(this GameObject o) where T : class
        {
            T component = o.GetComponentInParent<T>();
            Validate.NotNull<T>(component, o.name + " did not have a component of type " + typeof(T) + " in its parent");

            return component;
        }
    }
}
