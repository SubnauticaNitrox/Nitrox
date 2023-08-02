using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using ProtoBuf;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    [Serializable]
    [DataContract]
    [ProtoContract] // REQUIRED as the game serializes/deserializes phasing entities in batches when moving around the map.
    public class NitroxEntity : MonoBehaviour, IProtoTreeEventListener
    {
        private static readonly Dictionary<NitroxId, GameObject> gameObjectsById = new();

        [DataMember(Order = 1)]
        [ProtoMember(1)]
        public NitroxId Id;

        private NitroxEntity() // Default for Proto
        {
        }

        public static IEnumerable<KeyValuePair<NitroxId, GameObject>> GetGameObjects()
        {
            return gameObjectsById;
        }

        public static GameObject RequireObjectFrom(NitroxId id)
        {
            Optional<GameObject> gameObject = GetObjectFrom(id);
            Validate.IsPresent(gameObject, $"Game object required from id: {id}");
            return gameObject.Value;
        }

        public static Optional<GameObject> GetObjectFrom(NitroxId id)
        {
            if (id == null)
            {
                return Optional.Empty;
            }

            if (!gameObjectsById.TryGetValue(id, out GameObject gameObject))
            {
                return Optional.Empty;
            }

            // Nullable incase game object is marked as destroyed
            return Optional.OfNullable(gameObject);
        }

        public static Dictionary<NitroxId, GameObject> GetObjectsFrom(HashSet<NitroxId> ids)
        {
            return ids.Select(id => new KeyValuePair<NitroxId, GameObject>(id, gameObjectsById.GetOrDefault(id, null)))
                      .Where(keyValue => keyValue.Value)
                      .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public static bool TryGetObjectFrom(NitroxId id, out GameObject gameObject)
        {
            gameObject = null;
            return id != null && gameObjectsById.TryGetValue(id, out gameObject);
        }

        public static bool TryGetComponentFrom<T>(NitroxId id, out T component) where T : Component
        {
            component = null;
            return id != null && gameObjectsById.TryGetValue(id, out GameObject gameObject) &&
                   gameObject.TryGetComponent(out component);
        }

        public static void SetNewId(GameObject gameObject, NitroxId id)
        {
            Validate.IsTrue(gameObject);
            Validate.NotNull(id);

            if (gameObject.TryGetComponent(out NitroxEntity entity))
            {
                gameObjectsById.Remove(entity.Id);
            }
            else
            {
                entity = gameObject.AddComponent<NitroxEntity>();
            }

            entity.Id = id;
            gameObjectsById[id] = gameObject;
        }

        public static NitroxId GenerateNewId(GameObject gameObject)
        {
            Validate.IsTrue(gameObject);

            NitroxId id = new();
            SetNewId(gameObject, id);
            return id;
        }

        public static NitroxId GetIdOrGenerateNew(GameObject gameObject)
        {
            Validate.IsTrue(gameObject);

            if (gameObject.TryGetComponent(out NitroxEntity entity))
            {
                return entity.Id;
            }

            NitroxId id = new();
            SetNewId(gameObject, id);
            return id;
        }

        public static void RemoveFrom(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out NitroxEntity entity) && entity.Id != null)
            {
                gameObjectsById.Remove(entity.Id);
                DestroyImmediate(entity);
            }
        }

        /// <summary>
        /// Removes the <see cref="NitroxEntity"/> from the global directory and set's its <see cref="Id"/> to null.
        /// </summary>
        public void Remove()
        {
            if (Id != null)
            {
                gameObjectsById.Remove(Id);
                Id = null;
            }
        }

        public void Start()
        {
            // Just in case this object comes to life via serialization
            if (Id != null)
            {
                gameObjectsById[Id] = gameObject;
            }
        }

        public void OnProtoSerializeObjectTree(ProtobufSerializer _)
        {
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer _)
        {
            gameObjectsById[Id] = gameObject;
        }
    }
}
