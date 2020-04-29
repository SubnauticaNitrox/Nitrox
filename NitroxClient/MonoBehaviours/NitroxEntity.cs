using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using ProtoBuf;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    [Serializable]
    [ProtoContract]
    public class NitroxEntity : MonoBehaviour, IProtoTreeEventListener
    {
        private static Dictionary<NitroxId, GameObject> gameObjectsById = new Dictionary<NitroxId, GameObject>();

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
            Validate.IsPresent(gameObject, "Game object required from id: " + id);
            return gameObject.Value;
        }

        public static Optional<GameObject> GetObjectFrom(NitroxId id)
        {
            if (id == null)
            {
                return Optional.Empty;
            }

            GameObject gameObject;
            if (!gameObjectsById.TryGetValue(id, out gameObject))
            {
                return Optional.Empty;
            }

            // Nullable incase game object is marked as destroyed
            return Optional.OfNullable(gameObject);
        }

        public static void SetNewId(GameObject gameObject, NitroxId id)
        {
            Validate.NotNull(gameObject);
            Validate.NotNull(id);

            NitroxEntity entity = gameObject.GetComponent<NitroxEntity>();
            if (entity != null)
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

        public static NitroxId GetId(GameObject gameObject)
        {
            NitroxEntity entity = gameObject.GetComponent<NitroxEntity>();
            if (entity)
            {
                return entity.Id;
            }

            NitroxId newId = new NitroxId();
            SetNewId(gameObject, newId);

            return newId;
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
