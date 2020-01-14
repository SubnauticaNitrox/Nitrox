using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;
using ProtoBuf;

namespace NitroxClient.MonoBehaviours
{
    [Serializable]
    [ProtoContract]
    public class NitroxIdentifier : MonoBehaviour, IProtoTreeEventListener
    {
        [ProtoMember(1)]
        public NitroxId Id;

        private static Dictionary<NitroxId, GameObject> gameObjectsById = new Dictionary<NitroxId, GameObject>();

        private NitroxIdentifier() // Default for Proto
        {}

        public void Start()
        {
            // Just in case this object comes to life via serialization
            gameObjectsById[Id] = gameObject;
        }

        public static GameObject RequireObjectFrom(NitroxId id)
        {
            Optional<GameObject> gameObject = GetObjectFrom(id);
            Validate.IsPresent(gameObject, "Game object required from id: " + id);
            return gameObject.Get();
        }

        public static Optional<GameObject> GetObjectFrom(NitroxId id)
        {
            if (id == null)
            {
                return Optional<GameObject>.Empty();
            }

            GameObject gameObject;

            if (!gameObjectsById.TryGetValue(id, out gameObject))
            {
                return Optional<GameObject>.Empty();
            }

            // Nullable incase game object is marked as destroyed
            return Optional<GameObject>.OfNullable(gameObject);
        }

        public static void SetNewId(GameObject gameObject, NitroxId id)
        {
            Validate.NotNull(gameObject);
            Validate.NotNull(id);

            NitroxIdentifier identifier = gameObject.GetComponent<NitroxIdentifier>();

            if (identifier != null)
            {
                gameObjectsById.Remove(identifier.Id);
            }
            else
            {
                identifier = gameObject.AddComponent<NitroxIdentifier>();
            }

            identifier.Id = id;
            gameObjectsById[id] = gameObject;
        }

        public static NitroxId GetId(GameObject gameObject)
        {
            NitroxIdentifier identifier = gameObject.GetComponent<NitroxIdentifier>();

            if(identifier != null)
            {
                return identifier.Id;
            }

            NitroxId newId = new NitroxId();
            SetNewId(gameObject, newId);

            return newId;
        }

        public void OnProtoSerializeObjectTree(ProtobufSerializer _)
        {}

        public void OnProtoDeserializeObjectTree(ProtobufSerializer _)
        {
            gameObjectsById[Id] = gameObject;
        }
    }
}
