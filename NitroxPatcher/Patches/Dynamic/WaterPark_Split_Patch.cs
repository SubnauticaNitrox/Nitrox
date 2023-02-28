using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Bases.New;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When two WaterParks are separated (a WaterPark piece in between them was destructed), gives the newly created WaterPark a NitroxEntity.
/// </summary>
public class WaterPark_Split_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => WaterPark.Split(default, default));
    private static SavedInteriorPiece NewWaterPark => BuildingTester.Main.NewWaterPark;

    public static void Prefix(WaterPark bottomWaterPark, WaterPark topWaterPark)
    {
        WaterPark newWaterPark = null;
        if (bottomWaterPark.TryGetComponent(out NitroxEntity originalEntity))
        {
            newWaterPark = topWaterPark;
        } else if (topWaterPark.TryGetComponent(out originalEntity))
        {
            newWaterPark = bottomWaterPark;
        }
        if (newWaterPark)
        {
            NitroxId newId = NewWaterPark?.NitroxId ?? new();
            NitroxEntity.SetNewId(newWaterPark.gameObject, newId);
            // If it was null beforehand, it means that the local player is responsible for destructing the WaterPark
            // If it was already set, it means that the local player is remotely destructing the WaterPark
            if (NewWaterPark == null)
            {
                BuildingTester.Main.NewWaterPark = NitroxInteriorPiece.From(newWaterPark);
            }
            Log.Debug($"Splitting two WaterParks, original WaterPark NitroxEntity: {originalEntity.Id}, new WaterPark NitroxEntity: {newId}");
        }
        else
        {
            Log.Error("Couldn't find an original WaterPark NitroxEntity");
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
