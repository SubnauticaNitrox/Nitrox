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
            if (!cellObject)
            {
                continue;
            }

            foreach (Transform child in cellObject)
            {
                // We only want to save NitroxIds
                if (child.gameObject.TryGetComponent(out NitroxEntity nitroxEntity))
                {
                    ObjectKey key = GetObjectKey(child.gameObject);
                    nitroxIdByObjectKey[key] = nitroxEntity.Id;
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
        ObjectKey key = new(gameObject.name,
                            gameObject.transform.parent.position,
                            gameObject.transform.localPosition,
                            gameObject.transform.localRotation);
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

    public static ObjectKey GetObjectKey(GameObject gameObject)
    {
        return new ObjectKey(gameObject.name,
                             gameObject.transform.parent.position,
                             gameObject.transform.localPosition,
                             gameObject.transform.localRotation);
    }

    public readonly struct ObjectKey : IEquatable<ObjectKey>
    {
        private readonly string name;
        private readonly Vector3 parentCellPosition;
        private readonly Vector3 position;
        private readonly Quaternion rotation;

        public ObjectKey(string name, Vector3 parentCellPosition, Vector3 position, Quaternion rotation)
        {
            this.name = name;
            this.parentCellPosition = parentCellPosition;
            this.position = position;
            this.rotation = rotation;
        }

        public bool Equals(ObjectKey other)
        {
            return name == other.name &&
                   Equals(parentCellPosition, other.parentCellPosition) &&
                   Equals(position, other.position) &&
                   Equals(rotation, other.rotation);
        }

        public bool Equals(Vector3 _position, Vector3 otherPosition)
        {
            return Vector3.SqrMagnitude(_position - otherPosition) < 0.1f;
        }

        public bool Equals(Quaternion _rotation, Quaternion otherRotation)
        {
            return Math.Abs(Quaternion.Angle(_rotation, otherRotation)) < 0.1f;
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
                hashCode = (hashCode * 397) ^ parentCellPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ position.GetHashCode();
                hashCode = (hashCode * 397) ^ rotation.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"[Name: {name}, Position: {position}, Rotation: {rotation}]";
        }
    }
}
