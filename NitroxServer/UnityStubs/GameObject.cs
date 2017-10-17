using System;
using System.Collections.Generic;

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

        private Dictionary<Type, object> components = new Dictionary<Type, object>();

        public GameObject(ProtobufSerializer.GameObjectData goData)
        {
            IsActive = goData.IsActive;
            Layer = goData.Layer;
            Tag = goData.Tag;
            Id = goData.Id;
            ClassId = goData.ClassId;
            Parent = goData.Parent;
        }

        public void AddComponent(object component, Type componentType)
        {
            components.Add(componentType, component);
        }

        public object GetComponent(Type type)
        {
            if (components.ContainsKey(type))
            {
                return components[type];
            }

            return null;
        }

        public T GetComponent<T>()
        {
            if(components.ContainsKey(typeof(T)))
            {
                return (T)components[typeof(T)];
            }

            return default(T);
        }

        public bool HasComponent<T>()
        {
            return components.ContainsKey(typeof(T));
        }
    }
}
