using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.Helper;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.Unity.Helper
{
    public static class GameObjectHelper
    {
        public static bool TryGetComponentInChildren<T>(this GameObject go, out T component, bool includeInactive = false) where T : Component
        {
            component = go.GetComponentInChildren<T>(includeInactive);
            return component;
        }

        public static bool TryGetComponentInParent<T>(this GameObject go, out T component) where T : Component
        {
            component = go.GetComponentInParent<T>();
            return component;
        }

        public static bool TryGetComponentInChildren<T>(this Component co, out T component, bool includeInactive = false) where T : Component => TryGetComponentInChildren(co.gameObject, out component, includeInactive);
        public static bool TryGetComponentInParent<T>(this Component co, out T component) where T : Component => TryGetComponentInParent(co.gameObject, out component);

        public static T RequireComponent<T>(this GameObject o) where T : Component
        {
            T component = o.GetComponent<T>();
            Validate.IsTrue(component, $"{o.name} did not have a component of type {typeof(T)}");

            return component;
        }

        public static T RequireComponentInChildren<T>(this GameObject o, bool includeInactive = false) where T : Component
        {
            T component = o.GetComponentInChildren<T>(includeInactive);
            Validate.IsTrue(component, $"{o.name} did not have a component of type {typeof(T)} in its children");

            return component;
        }

        public static T RequireComponentInParent<T>(this GameObject o) where T : Component
        {
            T component = o.GetComponentInParent<T>();
            Validate.IsTrue(component, $"{o.name} did not have a component of type {typeof(T)} in its parent");

            return component;
        }

        public static T RequireComponent<T>(this Component co) where T : Component => RequireComponent<T>(co.gameObject);
        public static T RequireComponentInChildren<T>(this Component co, bool includeInactive = false) where T : Component => RequireComponentInChildren<T>(co.gameObject, includeInactive);
        public static T RequireComponentInParent<T>(this Component co) where T : Component => RequireComponentInParent<T>(co.gameObject);

        public static Transform RequireTransform(this Transform tf, string name)
        {
            Transform child = tf.Find(name);

            if (!child)
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
            Validate.IsTrue(go, "No global GameObject found with " + name + "!");

            return go;
        }

        /// <summary>
        ///     Returns null if Unity has marked this object as dead.
        /// </summary>
        /// <param name="obj">Unity <see cref="UnityEngine.Object" /> to check if alive.</param>
        /// <typeparam name="TObject">Type of Unity object that can be marked as either alive or dead.</typeparam>
        /// <returns>The <see cref="UnityEngine.Object" /> if alive or null if dead.</returns>
        public static TObject AliveOrNull<TObject>(this TObject obj) where TObject : Object
        {
            // Unity checks if the object is alive like this. Do NOT use == null check.
            if (obj)
            {
                return obj;
            }

            return null;
        }

        public static string GetFullHierarchyPath(this Component component)
        {
            return component ? $"{component.gameObject.GetFullHierarchyPath()} -> {component.GetType().Name}.cs" : "";
        }

        public static string GetHierarchyPath(this GameObject go, GameObject end)
        {
            if (!go || go == end)
            {
                return "";
            }

            if (!go.transform.parent)
            {
                return go.name;
            }

            StringBuilder sb = new(go.name);
            for (GameObject gameObject = go.transform.parent.gameObject;
                 gameObject && gameObject != end;
                 gameObject = gameObject.transform.parent ? gameObject.transform.parent.gameObject : null)
            {
                sb.Insert(0, $"{gameObject.name}/");
            }

            return sb.ToString();
        }

        public static bool TryGetComponentInAscendance<T>(this Transform transform, int degree, out T component)
        {
            while (degree > 0)
            {
                if (!transform.parent)
                {
                    component = default;
                    return false;
                }
                transform = transform.parent;
                degree--;
            }
            return transform.TryGetComponent(out component);
        }
    }
}
