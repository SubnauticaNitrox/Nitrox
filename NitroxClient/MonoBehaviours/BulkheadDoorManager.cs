using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Gives the bulkhead doors of a <see cref="Base"/> their <see cref="BulkheadDoorEntity"/> id, and assigns it back after each <see cref="Base.RebuildGeometry"/>.
/// </summary>
/// <remarks>
/// Doors are created by <see cref="Base.BuildCorridorGeometry"/> and have no identity of their own, unlike wreck doors which are spawned from server entities.
/// The game identifies a door by the <see cref="Base.Face"/> it sits on, so we do the same. The face cell is stored relative to the anchor
/// (see <see cref="Base.anchor"/>) so that it stays valid when the base grows.
/// </remarks>
public class BulkheadDoorManager : MonoBehaviour
{
    private const int DIRECTION_COUNT = 6;

    private Base @base;
    private readonly Dictionary<NitroxBaseFace, BulkheadDoorEntity> doorsByFace = new();

    public void Awake()
    {
        if (!TryGetComponent(out @base))
        {
            Log.Error($"Tried adding a {nameof(BulkheadDoorManager)} to a GameObject that isn't a base, deleting it.");
            Destroy(this);
            return;
        }
        @base.onPostRebuildGeometry += OnPostRebuildGeometry;
    }

    public void OnDestroy()
    {
        if (@base)
        {
            @base.onPostRebuildGeometry -= OnPostRebuildGeometry;
        }
    }

    private void OnPostRebuildGeometry(Base _)
    {
        // The rebuilt doors are new GameObjects, but the game restored their state from the base's faces
        AssignIds(false);
    }

    /// <summary>
    /// Called when a player toggled a door of this base.
    /// </summary>
    public void OnDoorStateChanged(BulkheadDoor door, bool opened)
    {
        BulkheadDoorMetadata metadata = new(opened);
        if (door.TryGetNitroxId(out NitroxId doorId))
        {
            this.Resolve<Entities>().BroadcastMetadataUpdate(doorId, metadata);
            return;
        }

        if (!@base.TryGetNitroxId(out NitroxId baseId))
        {
            Log.WarnOnce($"[{nameof(BulkheadDoorManager)}] Base {@base.name} has no NitroxEntity, its bulkhead doors can't be synced");
            return;
        }
        if (!TryGetFace(door, out NitroxBaseFace face))
        {
            Log.Warn($"[{nameof(BulkheadDoorManager)}] Couldn't find the face of bulkhead door {door.name} in base {baseId}");
            return;
        }

        // First interaction with this door: it gets an entity (state included), the server dedupes concurrent registrations
        BulkheadDoorEntity entity = new(new NitroxId(), baseId, face, metadata);
        doorsByFace[face] = entity;
        NitroxEntity.SetNewId(door.gameObject, entity.Id);
        this.Resolve<IPacketSender>().Send(new BulkheadDoorRegistered(entity));
    }

    /// <summary>
    /// Registers (or replaces) the entity of a door, and applies its state.
    /// </summary>
    public void RegisterDoor(BulkheadDoorEntity entity)
    {
        doorsByFace[entity.Face] = entity;
        AssignIds(true);
    }

    public void LoadDoors(IEnumerable<BulkheadDoorEntity> doorEntities)
    {
        doorsByFace.Clear();
        foreach (BulkheadDoorEntity doorEntity in doorEntities)
        {
            doorsByFace[doorEntity.Face] = doorEntity;
        }
        AssignIds(true);
    }

    /// <returns>The registered doors still having a bulkhead on their face</returns>
    public List<BulkheadDoorEntity> GetSavedDoors()
    {
        return doorsByFace.Values.Where(door => IsBulkhead(door.Face)).ToList();
    }

    private void AssignIds(bool applyState)
    {
        foreach (BulkheadDoorEntity entity in doorsByFace.Values)
        {
            Transform faceObject = @base.FindFaceObject(ToBaseFace(entity.Face));
            if (!faceObject || !faceObject.TryGetComponentInChildren(out BulkheadDoor door, true))
            {
                continue;
            }

            NitroxEntity.SetNewId(door.gameObject, entity.Id);
            if (applyState && entity.Metadata is BulkheadDoorMetadata metadata && door.opened != metadata.Opened)
            {
                using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
                using (PacketSuppressor<BulkheadDoorRegistered>.Suppress())
                {
                    door.SetState(metadata.Opened);
                }
            }
        }
    }

    private bool TryGetFace(BulkheadDoor door, out NitroxBaseFace face)
    {
        face = default;

        // Cell objects are direct children of the base, the face piece holding the door is one of their children
        Transform cellObject = door.transform;
        while (cellObject && cellObject.parent != @base.transform)
        {
            cellObject = cellObject.parent;
        }
        if (!cellObject || @base.FindCellObject(cellObject) is not Int3 cell)
        {
            return false;
        }

        for (int direction = 0; direction < DIRECTION_COUNT; direction++)
        {
            Base.Face candidate = new(cell, (Base.Direction)direction);
            if (!Base.IsBulkhead(@base.GetFace(candidate)))
            {
                continue;
            }

            Transform faceObject = @base.FindFaceObject(candidate);
            if (faceObject && door.transform.IsChildOf(faceObject))
            {
                candidate.cell -= @base.GetAnchor();
                face = candidate.ToDto();
                return true;
            }
        }
        return false;
    }

    private bool IsBulkhead(NitroxBaseFace face) => Base.IsBulkhead(@base.GetFace(ToBaseFace(face)));

    private Base.Face ToBaseFace(NitroxBaseFace face)
    {
        Base.Face baseFace = face.ToUnity();
        baseFace.cell += @base.GetAnchor();
        return baseFace;
    }

    public static void RestoreDoors(IEnumerable<BulkheadDoorEntity> doorEntities, Base @base)
    {
        BulkheadDoorManager manager = @base.gameObject.EnsureComponent<BulkheadDoorManager>();
        manager.LoadDoors(doorEntities);
    }
}
