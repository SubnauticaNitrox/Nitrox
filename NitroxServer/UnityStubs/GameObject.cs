using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;
using NitroxServer.Serialization;

namespace NitroxServer.UnityStubs
{
    public class GameObject
    {
        public bool IsActive { get; }
        public int Layer { get; }
        public string Tag { get; }
        public string Id { get; }
        public string ClassId { get; }
        public string Parent { get; }

        public int TotalComponents => components.Count;

        private readonly Dictionary<Type, object> components = new Dictionary<Type, object>();

        public GameObject(GameObjectData goData)
        {
            IsActive = goData.IsActive;
            Layer = goData.Layer;
            Tag = goData.Tag;
            Id = goData.Id;
            ClassId = goData.ClassId;
            Parent = goData.Parent;
        }

        public override string ToString()
        {
            components.TryGetValue(typeof(NitroxTransform), out object transform); // Honestly this should never be null every gameObject has a Transform

            return string.Format("Id: {0}, Class Id: {1}, Transform: {2}", Id, ClassId, transform as NitroxTransform);
        }

        public void AddComponent(object component, Type componentType)
        {
            components.Add(componentType, component);
        }

        public object GetComponent(Type type)
        {
            if (components.TryGetValue(type, out object res))
            {
                return res;
            }

            return null;
        }

        public T GetComponent<T>()
        {
            return (T)GetComponent(typeof(T));
        }

        public bool HasComponent<T>()
        {
            return components.ContainsKey(typeof(T));
        }
    }
}
