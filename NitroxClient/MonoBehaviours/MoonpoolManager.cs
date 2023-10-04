using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
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
    private Entities entities;

    private Base @base;
    private NitroxId baseId;
    private Dictionary<Int3, MoonpoolEntity> moonpoolsByCell;
    public MoonpoolEntity LatestRegisteredMoonpool { get; private set; }

    public void Awake()
    {
        entities = this.Resolve<Entities>();

        if (!TryGetComponent(out @base))
        {
            Log.Error($"Tried adding a {nameof(MoonpoolManager)} to a GameObject that isn't a bases, deleting it.");
            Destroy(this);
            return;
        }
        @base.TryGetNitroxId(out baseId);
        moonpoolsByCell = new();
        @base.onPostRebuildGeometry += OnPostRebuildGeometry;
    }

    public void OnDestroy()
    {
        @base.onPostRebuildGeometry -= OnPostRebuildGeometry;
    }
    
    public void LateAssignNitroxEntity(NitroxId baseId)
    {
        this.baseId = baseId;
        foreach (MoonpoolEntity moonpoolEntity in moonpoolsByCell.Values)
        {
            moonpoolEntity.ParentId = baseId;
            moonpoolEntity.Id = new(); // Generate a new id in case
        }
    }

    public void OnPostRebuildGeometry(Base _)
    {
        foreach (KeyValuePair<Int3, MoonpoolEntity> moonpoolEntry in moonpoolsByCell)
        {
            AssignNitroxEntityToMoonpool(moonpoolEntry.Key, moonpoolEntry.Value.Id);
        }
    }

    private void AssignNitroxEntityToMoonpool(Int3 absoluteCell, NitroxId moonpoolId, TaskResult<Optional<GameObject>> result = null)
    {
        Int3 relativeCell = Relative(absoluteCell);
        Transform baseCellTransform = @base.GetCellObject(relativeCell);
        if (!baseCellTransform)
        {
            Log.Warn($"[{nameof(AssignNitroxEntityToMoonpool)}] CellObject not found for RelativeCell: {relativeCell}, AbsoluteCell: {absoluteCell}");
            return;
        }
        if (baseCellTransform.TryGetComponentInChildren(out VehicleDockingBay vehicleDockingBay, true))
        {
            result?.Set(vehicleDockingBay.gameObject);
            NitroxEntity.SetNewId(vehicleDockingBay.gameObject, moonpoolId);
        }
    }

    public Optional<GameObject> RegisterMoonpool(Transform constructableTransform, NitroxId moonpoolId)
    {
        Int3 absoluteCell = Absolute(constructableTransform.position);
        moonpoolsByCell[absoluteCell] = new(moonpoolId, baseId, absoluteCell.ToDto());
        TaskResult<Optional<GameObject>> resultObject = new();
        AssignNitroxEntityToMoonpool(absoluteCell, moonpoolId, resultObject);
        LatestRegisteredMoonpool = moonpoolsByCell[absoluteCell];
        return resultObject.Get();
    }

    public NitroxId DeregisterMoonpool(Transform constructableTransform)
    {
        Int3 absoluteCell = Absolute(constructableTransform.position);
        if (moonpoolsByCell.TryGetValue(absoluteCell, out MoonpoolEntity moonpoolEntity))
        {
            moonpoolsByCell.Remove(absoluteCell);
            return moonpoolEntity.Id;
        }
        return null;
    }

    public void LoadMoonpools(IEnumerable<MoonpoolEntity> moonpoolEntities)
    {
        moonpoolsByCell.Clear();
        foreach (MoonpoolEntity moonpoolEntity in moonpoolEntities)
        {
            moonpoolsByCell[moonpoolEntity.Cell.ToUnity()] = moonpoolEntity;
        }
    }

    public IEnumerator SpawnVehicles()
    {
        foreach (MoonpoolEntity moonpoolEntity in moonpoolsByCell.Values)
        {
            VehicleWorldEntity moonpoolVehicleEntity = moonpoolEntity.ChildEntities.OfType<VehicleWorldEntity>().FirstOrFallback(null);
            if (moonpoolVehicleEntity != null)
            {
                yield return entities.SpawnEntityAsync(moonpoolVehicleEntity);
            }
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

    public List<MoonpoolEntity> GetSavedMoonpools()
    {
        return moonpoolsByCell.Values.ToList();
    }

    public Dictionary<NitroxId, NitroxInt3> GetMoonpoolsUpdate()
    {
        return moonpoolsByCell.ToDictionary(entry => entry.Value.Id, entry => entry.Key.ToDto());
    }

    [Conditional("DEBUG")]
    public void PrintDebug()
    {
        Log.Debug($"MoonpoolManager's registered moonpools (anchor: {@base.GetAnchor()}):");
        foreach (MoonpoolEntity moonpoolEntity in moonpoolsByCell.Values)
        {
            Int3 absoluteCell = moonpoolEntity.Cell.ToUnity();
            Int3 baseCell = Relative(moonpoolEntity.Cell.ToUnity());
            Log.Debug($"AbsoluteCell: {absoluteCell}, BaseCell: {baseCell}, id: {moonpoolEntity.Id}");
        }
    }

    public static IEnumerator RestoreMoonpools(IEnumerable<MoonpoolEntity> moonpoolEntities, Base @base)
    {
        MoonpoolManager moonpoolManager = @base.gameObject.EnsureComponent<MoonpoolManager>();
        moonpoolManager.LoadMoonpools(moonpoolEntities);
        moonpoolManager.OnPostRebuildGeometry(@base);
        yield return moonpoolManager.SpawnVehicles();
    }
}
