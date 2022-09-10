using System;
using System.Collections.Generic;
using NitroxClient.GameLogic.Bases.Spawning.BasePiece;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases;

/**
     * When new bases are constructed it will sometimes clear all of the pieces 
     * and reconnect them.  (This is primarily for visual purposes so it can change
     * out the model if required.)  When these pieces are cleared, we need to persist
     * them so that we can update the newly placed pieces with the proper id. 
     */
public class GeometryRespawnManager
{
    public readonly HashSet<NitroxId> NitroxIdsToIgnore = new();
    private readonly Dictionary<ObjectKey, NitroxId> nitroxIdByObjectKey = new();

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
                            ObjectKey key = GetObjectKey(child.gameObject);
                            nitroxIdByObjectKey[key] = id;
                        }
                    }
                }
            }
        }
    }

    public void BaseObjectRespawned(GameObject gameObject)
    {
        ObjectKey key = GetObjectKey(gameObject);
        UpdateBasePieceIdIfPossible(gameObject, key);
    }

    public void BaseFaceRespawned(GameObject gameObject, Int3 faceCell, Base.Direction? direction)
    {
        // face based pieces are handled a bit differently on respawn as the BaseDeconstructable is not
        // yet attached to the game object.  So we'll pass in the known fields instead.
        ObjectKey key = new(gameObject.name, gameObject.transform.position, faceCell, direction);
        UpdateBasePieceIdIfPossible(gameObject, key);
    }

    private void UpdateBasePieceIdIfPossible(GameObject gameObject, ObjectKey key)
    {
        if (nitroxIdByObjectKey.TryGetValue(key, out NitroxId id))
        {
            nitroxIdByObjectKey.Remove(key);
            if (NitroxIdsToIgnore.Contains(id))
            {
                NitroxIdsToIgnore.Remove(id);
                return;
            }
            NitroxEntity.SetNewId(gameObject, id);
            // If this piece has a NitroxId, it probably had a spawn processor run for it
            BasePieceSpawnProcessor.ReRunSpawnProcessor(gameObject, id);
        }
    }

    private static ObjectKey GetObjectKey(GameObject gameObject)
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
        
        return new ObjectKey(gameObject.name, gameObject.transform.position, faceCell, faceDirection);
    }

    private readonly struct ObjectKey : IEquatable<ObjectKey>
    {
        private readonly string name;
        private readonly Vector3 position;
        private readonly Int3 faceCell;
        private readonly Base.Direction? direction;

        public ObjectKey(string name, Vector3 position, Int3 faceCell, Base.Direction? direction)
        {
            this.name = name;
            this.position = position;
            this.faceCell = faceCell;
            this.direction = direction;
        }
        
        public bool Equals(ObjectKey other)
        {
            return name == other.name && 
                   position.Equals(other.position) && 
                   faceCell.Equals(other.faceCell) && 
                   direction == other.direction;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ position.GetHashCode();
                hashCode = (hashCode * 397) ^ faceCell.GetHashCode();
                hashCode = (hashCode * 397) ^ direction.GetHashCode();
                return hashCode;
            }
        }
    }
}
