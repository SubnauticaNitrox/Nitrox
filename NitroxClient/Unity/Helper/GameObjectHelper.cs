using System;
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

        public static Transform RequireTransform(this Transform tf, string name)
        {
            Transform child = tf.Find(name);

            if (child == null)
            {
                throw new ArgumentNullException(tf + " does not contain \"" + name + "\"");
            }

            return child;
        }
        public static Transform RequireTransform(this GameObject go, string name) => go.transform.RequireTransform(name);
        public static Transform RequireTransform(this MonoBehaviour mb, string name) => mb.transform.RequireTransform(name);

        public static GameObject RequireGameObject(this Transform tf, string name) => tf.RequireTransform(name).gameObject;
        public static GameObject RequireGameObject(this GameObject go, string name) => go.transform.RequireGameObject(name);
        public static GameObject RequireGameObject(this MonoBehaviour mb, string name) => mb.transform.RequireGameObject(name);

        public static GameObject RequireGameObject(string name)
        {
            GameObject go = GameObject.Find(name);
            Validate.NotNull(go, "No global GameObject found with " + name + "!");

            return go;
        }
    }
}
