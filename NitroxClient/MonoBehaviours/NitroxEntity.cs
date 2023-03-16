using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NitroxClient.Unity.Helper;
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
        private static Dictionary<NitroxId, GameObject> gameObjectsById = new Dictionary<NitroxId, GameObject>();

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

        public static bool TryGetEntityFrom(GameObject gameObject, out NitroxEntity nitroxEntity)
        {
            nitroxEntity = null;
            return gameObject && gameObject.TryGetComponent(out nitroxEntity);
        }

        public static bool TryGetIdFrom(GameObject gameObject, out NitroxId nitroxId)
        {
            if (gameObject && gameObject.TryGetComponent(out NitroxEntity nitroxEntity))
            {
                nitroxId = nitroxEntity.Id;
                return true;
            }

            nitroxId = null;
            return false;
        }

        public static bool TryGetIdFrom(Component component, out NitroxId nitroxId)
        {
            if (component)
            {
                return TryGetIdFrom(component.gameObject, out nitroxId);
            }

            nitroxId = null;
            return false;
        }

        public static Optional<NitroxId> GetOptionalIdFrom(GameObject gameObject)
        {
            if (gameObject && gameObject.TryGetComponent(out NitroxEntity nitroxEntity))
            {
                return Optional.Of(nitroxEntity.Id);
            }

            return Optional.Empty;
        }

        public static Optional<NitroxId> GetOptionalIdFrom(Component component) => component ? GetOptionalIdFrom(component.gameObject) : Optional.Empty;

        public static NitroxEntity RequireEntityFrom(GameObject gameObject)
        {
            NitroxEntity nitroxEntity = gameObject.AliveOrNull()?.GetComponent<NitroxEntity>();
            Validate.IsTrue(nitroxEntity);
            return nitroxEntity;
        }

        public static NitroxEntity RequireEntityFrom(Component component) => RequireEntityFrom(component.gameObject);

        public static NitroxId RequireIdFrom(GameObject gameObject) => RequireEntityFrom(gameObject).Id;

        public static NitroxId RequireIdFrom(Component component) => RequireEntityFrom(component).Id;

        public static void SetNewId(GameObject gameObject, NitroxId id)
        {
            Validate.NotNull(gameObject);
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

        public static void RemoveFrom(GameObject gameObject)
        {
            NitroxEntity entity = gameObject.GetComponent<NitroxEntity>();

            if (entity)
            {
                gameObjectsById.Remove(entity.Id);
                Destroy(entity);
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
