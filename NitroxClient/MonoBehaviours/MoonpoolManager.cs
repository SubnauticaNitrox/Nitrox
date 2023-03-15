using System.Collections.Generic;
using System.Linq;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Saves the moonpools NitroxEntity of a <see cref="Base"/> to assign them back after each <see cref="Base.RebuildGeometry"/>.
/// </summary>
/// <remarks>
/// To recognize pieces even after the base is rebuilt, we use the base anchor (<see cref="Base.anchor"/>) to get an absolute cell value.
/// </remarks>
public class MoonpoolManager : MonoBehaviour
{
    private Base @base;
    private NitroxId baseId;
    private Dictionary<Int3, SavedMoonpool> moonpoolsByCell;
    private Dictionary<Int3, SavedMoonpool> shiftedMoonpools;

    public void Awake()
    {
        if (!TryGetComponent(out @base))
        {
            Log.Error($"Tried adding a {nameof(MoonpoolManager)} to a GameObject that isn't a bases, deleting it.");
            Destroy(this);
            return;
        }
        if (NitroxEntity.TryGetEntityFrom(@base.gameObject, out NitroxEntity baseEntity))
        {
            baseId = baseEntity.Id;
        }
        moonpoolsByCell = new();
        shiftedMoonpools = new();
        @base.onPostRebuildGeometry += OnPostRebuildGeometry;
    }

    public void OnDestroy()
    {
        @base.onPostRebuildGeometry -= OnPostRebuildGeometry;
    }
    
    public void LateAssignNitroxEntity(NitroxId baseId)
    {
        this.baseId = baseId;
        foreach (SavedMoonpool savedMoonpool in moonpoolsByCell.Values)
        {
            savedMoonpool.ParentId = baseId;
            savedMoonpool.NitroxId = new(); // Generate a new id in case
        }
    }

    public void OnPostRebuildGeometry(Base _)
    {
        foreach (KeyValuePair<Int3, SavedMoonpool> shiftedCell in shiftedMoonpools)
        {
            moonpoolsByCell[shiftedCell.Key] = shiftedCell.Value;
        }
        shiftedMoonpools.Clear();

        foreach (KeyValuePair<Int3, SavedMoonpool> savedMoonpool in moonpoolsByCell)
        {
            AssignNitroxEntityToMoonpool(savedMoonpool.Key, savedMoonpool.Value.NitroxId);
        }
    }

    private void AssignNitroxEntityToMoonpool(Int3 absoluteCell, NitroxId moonpoolId)
    {
        Int3 relativeCell = Relative(absoluteCell);
        Transform baseCellTransform = @base.GetCellObject(relativeCell);
        if (!baseCellTransform)
        {
            Log.Warn($"[{nameof(MoonpoolManager.AssignNitroxEntityToMoonpool)}] CellObject not found for RelativeCell: {relativeCell}, AbsoluteCell: {absoluteCell}");
            return;
        }
        if (baseCellTransform.TryGetComponentInChildren(out VehicleDockingBay vehicleDockingBay))
        {
            NitroxEntity.SetNewId(vehicleDockingBay.gameObject, moonpoolId);
        }
    }

    public void RegisterMoonpool(Transform constructableTransform, NitroxId moonpoolId)
    {
        Int3 absoluteCell = Absolute(constructableTransform.position);
        moonpoolsByCell[absoluteCell] = new()
        {
            NitroxId = moonpoolId,
            ParentId = baseId,
            Cell = absoluteCell.ToDto(),
        };
        AssignNitroxEntityToMoonpool(absoluteCell, moonpoolId);
    }

    public NitroxId DeregisterMoonpool(Transform constructableTransform)
    {
        Int3 absoluteCell = Absolute(constructableTransform.position);
        if (moonpoolsByCell.TryGetValue(absoluteCell, out SavedMoonpool savedMoonpool))
        {
            moonpoolsByCell.Remove(absoluteCell);
            return savedMoonpool.NitroxId;
        }
        return null;
    }

    public void LoadSavedMoonpools(List<SavedMoonpool> savedMoonpools)
    {
        moonpoolsByCell.Clear();
        foreach (SavedMoonpool savedMoonpool in savedMoonpools)
        {
            moonpoolsByCell[savedMoonpool.Cell.ToUnity()] = savedMoonpool;
        }
    }

    private Int3 Absolute(Vector3 position)
    {
        return Absolute(@base.WorldToGrid(position));
    }

    private Int3 Absolute(Int3 baseCell)
    {
        return baseCell - @base.GetAnchor();
    }

    private Int3 Relative(Int3 baseCell)
    {
        return baseCell + @base.GetAnchor();
    }

    public List<SavedMoonpool> GetSavedMoonpools()
    {
        return moonpoolsByCell.Values.ToList();
    }

    public void PrintDebug()
    {
        Log.Debug($"MoonpoolManager's registered moonpools (anchor: {@base.GetAnchor()}):");
        foreach (SavedMoonpool savedMoonpool in moonpoolsByCell.Values)
        {
            Int3 absoluteCell = savedMoonpool.Cell.ToUnity();
            Int3 baseCell = Relative(savedMoonpool.Cell.ToUnity());
            Log.Debug($"AbsoluteCell: {absoluteCell}, BaseCell: {baseCell}, id: {savedMoonpool.NitroxId}");
        }
    }
}
