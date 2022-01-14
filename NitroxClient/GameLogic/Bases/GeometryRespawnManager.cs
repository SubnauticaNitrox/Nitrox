using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases
{
    /**
     * When new bases are constructed it will sometimes clear all of the pieces 
     * and reconnect them.  (This is primarily for visual purposes so it can change
     * out the model if required.)  When these pieces are cleared, we need to persist
     * them so that we can update the newly placed pieces with the proper id. 
     */
    public class GeometryRespawnManager
    {
        public static readonly Dictionary<string, NitroxId> NitroxIdByObjectKey = new();
        public static readonly HashSet<NitroxId> NitroxIdsToIgnore = new();

        public void GeometryClearedForBase(Base baseObj)
        {
            Transform[] cellObjects = baseObj.cellObjects;

            if (cellObjects == null)
            {
                return;
            }

            foreach (Transform cellObject in cellObjects)
            {
                if (cellObject != null)
                {
                    for (int i = 0; i < cellObject.childCount; i++)
                    {
                        Transform child = cellObject.GetChild(i);

                        if (child != null && child.gameObject != null)
                        {
                            // Ensure there is already a nitrox id, we don't want to go creating one
                            // which happens if you call GetId directly and it is missing.
                            if (child.gameObject.GetComponent<NitroxEntity>() != null)
                            {
                                NitroxId id = NitroxEntity.GetId(child.gameObject);
                                string key = GetObjectKey(child.gameObject);
                                NitroxIdByObjectKey[key] = id;

                                Log.Debug("Clearing Base Geometry, storing id for later lookup: " + key + " " + id);
                            }
                        }
                    }
                }
            }
        }

        public void BaseObjectRespawned(GameObject gameObject)
        {
            string key = GetObjectKey(gameObject);
            UpdateBasePieceIdIfPossible(gameObject, key);
        }

        public void BaseFaceRespawned(GameObject gameObject, Int3 faceCell, Base.Direction? direction)
        {
            // face based pieces are handled a bit differently on respawn as the BaseDeconstructable is not
            // yet attached to the game object.  So we'll pass in the known fields instead.
            string key = getObjectKey(gameObject.name, gameObject.transform.position, faceCell, direction);
            UpdateBasePieceIdIfPossible(gameObject, key);
        }

        private void UpdateBasePieceIdIfPossible(GameObject gameObject, string key)
        {
            if (NitroxIdByObjectKey.TryGetValue(key, out NitroxId id))
            {
                NitroxIdByObjectKey.Remove(key);
                if (NitroxIdsToIgnore.Contains(id))
                {
                    Log.Debug($"When respawning geometry, found an ignored face-based id [Key: {key}, Id: {id}]");
                    NitroxIdsToIgnore.Remove(id);
                    return;
                }
                Log.Debug($"When respawning geometry, found face-based id to copy to new object [Key: {key}, Id: {id}]");
                NitroxEntity.SetNewId(gameObject, id);
            }
        }

        public string GetObjectKey(GameObject gameObject)
        {
            BaseDeconstructable deconstructable = gameObject.GetComponentInChildren<BaseDeconstructable>();

            Int3 faceCell = default(Int3);
            Base.Direction? faceDirection = null;

            if (deconstructable && deconstructable.face.HasValue)
            {
                Base.Face face = deconstructable.face.Value;
                faceCell = face.cell;
                faceDirection = face.direction;
            }

            return getObjectKey(gameObject.name, gameObject.transform.position, faceCell, faceDirection);
        }

        private string getObjectKey(string name, Vector3 position, Int3 faceCell, Base.Direction? direction)
        {
            string serializedDirection = (direction.HasValue) ? direction.Value.ToString() : "";
            return $"{name}__{position}__{faceCell}__{serializedDirection}";
        }

    }
}
