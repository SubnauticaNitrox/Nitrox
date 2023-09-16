using System.Reflection;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using static NitroxClient.GameLogic.Bases.BuildingHandler;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When two WaterParks are separated (a WaterPark piece in between them was destructed), gives the newly created WaterPark a NitroxEntity.
/// </summary>
public sealed partial class WaterPark_Split_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => WaterPark.Split(default, default));

    private static TemporaryBuildData Temp => BuildingHandler.Main.Temp;

    public static bool Prefix(WaterPark bottomWaterPark, WaterPark topWaterPark)
    {
        if (Temp.MovedChildrenIds == null)
        {
            return true;
        }

        // MovedChildrenIds is not null when this is called by a WaterParkDeconstructed packet
        // in which case we manually move the items to ensure they're in sync with the server
        foreach (NitroxId childId in Temp.MovedChildrenIds)
        {
            if (NitroxEntity.TryGetComponentFrom(childId, out WaterParkItem childItem))
            {
                bottomWaterPark.MoveItemTo(childItem, topWaterPark);
            }
        }
        return false;
    }

    public static void Postfix(WaterPark bottomWaterPark, WaterPark topWaterPark)
    {
        WaterPark newWaterPark = null;
        if (bottomWaterPark.TryGetComponent(out NitroxEntity originalEntity))
        {
            newWaterPark = topWaterPark;
        }
        else if (topWaterPark.TryGetComponent(out originalEntity))
        {
            newWaterPark = bottomWaterPark;
        }

        if (newWaterPark)
        {
            NitroxId newId = Temp.NewWaterPark?.Id ?? new();
            NitroxEntity.SetNewId(newWaterPark.gameObject, newId);
            // If it was null beforehand, it means that the local player is responsible for destructing the WaterPark
            // If it was already set, it means that the local player is remotely destructing the WaterPark
            if (Temp.NewWaterPark == null)
            {
                Temp.NewWaterPark = InteriorPieceEntitySpawner.From(newWaterPark);
                Temp.MovedChildrenIds = new();
                foreach (NitroxEntity childEntity in newWaterPark.itemsRoot.GetComponentsInChildren<NitroxEntity>(true))
                {
                    Temp.MovedChildrenIds.Add(childEntity.Id);
                }
            }
            Log.Debug($"Splitting two WaterParks, original WaterPark NitroxEntity: {originalEntity.Id}, new WaterPark NitroxEntity: {newId}");
        }
        else
        {
            Log.Error("Couldn't find an original WaterPark NitroxEntity");
        }
    }
}
