using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
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

        private static Dictionary<NitroxId, List<GameObject>> gameObjectsWaitingForParent = new Dictionary<NitroxId, List<GameObject>>();

        [ProtoMember(1)]
        public NitroxId Id;

        private NitroxEntity() // Default for Proto
        {
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

        public static bool AddToParent(NitroxId parentId, GameObject gameObject)
        {
            try
            {
                Optional<GameObject> parentObject = GetObjectFrom(parentId);
                if (parentObject.IsPresent())
                {
                    gameObject.transform.SetParent(parentObject.Get().transform, true);
                }
                else
                {
                    List<GameObject> childGameObjects;
                    if (!gameObjectsWaitingForParent.TryGetValue(parentId, out childGameObjects))
                    {
                        childGameObjects = new List<GameObject>();
                        gameObjectsWaitingForParent.Add(parentId, childGameObjects);
                    }

                    childGameObjects.Add(gameObject);
                }
            }
            catch (Exception ex)
            {
                NitroxModel.Logger.Log.Error("Errored on NitroxEntity.AddToParent: {0}, {1}, Exception Thrown: {2}", parentId, gameObject.name, ex);
                return false;
            }
            
            return true;
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

            List<GameObject> childGameObjects = new List<GameObject>();

            if (gameObjectsWaitingForParent.TryGetValue(id, out childGameObjects))
            {
                foreach (GameObject childGameObject in childGameObjects)
                {
                    if (childGameObject == null)
                    {
                        continue; // Child GameObject was deleted by the game?
                    }

                    childGameObject.transform.SetParent(gameObject.transform, true);
                    if (childGameObject.GetComponent<LargeWorldEntity>() != null)
                    {
                        LargeWorldEntity.Register(childGameObject);
                    }
                }

                gameObjectsWaitingForParent.Remove(id);
            }

            entity.Id = id;
            gameObjectsById[id] = gameObject;
        }

        public static NitroxId GetId(GameObject gameObject)
        {
            NitroxEntity entity = gameObject.GetComponent<NitroxEntity>();
            if (entity != null)
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
            gameObjectsById[Id] = gameObject;
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
