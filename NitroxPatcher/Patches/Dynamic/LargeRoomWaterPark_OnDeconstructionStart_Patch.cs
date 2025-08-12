using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using static NitroxClient.GameLogic.Bases.BuildingHandler;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts the distribution of fishes to other large room water parks when done by local player,
/// and applies the remote modification when a packet triggered it.
/// </summary>
public sealed partial class LargeRoomWaterPark_OnDeconstructionStart_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((LargeRoomWaterPark t) => t.OnDeconstructionStart());

    private static TemporaryBuildData Temp => BuildingHandler.Main.Temp;

    public static Dictionary<LargeRoomWaterPark, List<WaterParkItem>> MovedItemsByWaterPark;

    public static bool Prefix(LargeRoomWaterPark __instance)
    {
        MovedItemsByWaterPark = [];

        // locally deconstructing the water park
        if (Temp.MovedChildrenIdsByNewHostId == null)
        {
            return true;
        }
        
        // code from the original function
        if (__instance.size == 1)
        {
            return false;
        }
        __instance.size = 1;
        __instance.segments.Clear();
        __instance.segments.Add(__instance);
        __instance._rootWaterPark = __instance;

        // now operating the reparenting with the remote data
        foreach (KeyValuePair<NitroxId, List<NitroxId>> entry in Temp.MovedChildrenIdsByNewHostId)
        {
            if (!NitroxEntity.TryGetComponentFrom(entry.Key, out LargeRoomWaterPark parentWaterPark))
            {
                Log.Error($"[{nameof(LargeRoomWaterPark_OnDeconstructionStart_Patch)}] Could not find {nameof(LargeRoomWaterPark)} with id {entry.Key}");
                continue;
            }

            foreach (NitroxId childId in entry.Value)
            {
                if (NitroxEntity.TryGetComponentFrom(childId, out WaterParkItem waterParkItem))
                {
                    waterParkItem.SetWaterPark(parentWaterPark);
                }
                else
                {
                    Log.Error($"[{nameof(LargeRoomWaterPark_OnDeconstructionStart_Patch)}] Could not find {nameof(WaterParkItem)} with id {entry.Key}");
                }
            }
        }

        return false;
    }

    /*
     * waterParkItem.SetWaterPark(largeRoomWaterPark);
     * LargeRoomWaterPark_OnDeconstructionStart_Patch.RegisterReparenting(waterParkItem, largeRoomWaterPark);   <--- [INSERTED LINE]
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Callvirt, Reflect.Method((WaterParkItem t) => t.SetWaterPark(default)))
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldloc_0),
                                                new CodeInstruction(OpCodes.Ldloc_S, (byte)6),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => RegisterReparenting(default, default)))
                                            ]).InstructionEnumeration();
    }

    public static void RegisterReparenting(WaterParkItem waterParkItem, LargeRoomWaterPark largeRoomWaterPark)
    {
        if (!MovedItemsByWaterPark.TryGetValue(largeRoomWaterPark, out List<WaterParkItem> children))
        {
            children = MovedItemsByWaterPark[largeRoomWaterPark] = [];
        }

        children.Add(waterParkItem);
    }

    public static void Finalizer()
    {
        Dictionary<NitroxId, List<NitroxId>> movedChildrenIdsByNewHostId = [];

        foreach (KeyValuePair<LargeRoomWaterPark, List<WaterParkItem>> entry in MovedItemsByWaterPark)
        {
            if (!entry.Key.TryGetIdOrWarn(out NitroxId parentId))
            {
                continue;
            }

            List<NitroxId> childrenIds = [];

            foreach (WaterParkItem waterParkItem in entry.Value)
            {
                if (waterParkItem.TryGetIdOrWarn(out NitroxId childId))
                {
                    childrenIds.Add(childId);
                }
            }

            movedChildrenIdsByNewHostId.Add(parentId, childrenIds);
        }

        if (movedChildrenIdsByNewHostId.Count > 0)
        {
            Temp.MovedChildrenIdsByNewHostId = movedChildrenIdsByNewHostId;
        }

        MovedItemsByWaterPark = [];
    }
}
