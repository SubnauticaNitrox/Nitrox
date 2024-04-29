using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents fishing from being born in a WaterPark if the said WaterPark is not simulated by the local player.
/// Syncs fish birth.
/// </summary>
public sealed partial class WaterParkCreature_BornAsync_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method(() => WaterParkCreature.BornAsync(default, default, default)));

    /*
     * MODIFIED:
     * if (creaturePrefabReference == null || !creaturePrefabReference.RuntimeKeyIsValid())
     * BECOMES
     * if (creaturePrefabReference == null || !creaturePrefabReference.RuntimeKeyIsValid() || !WaterParkCreature_BornAsync_Patch.CanBeBorn(waterPark))
     * 
     * INSERTED:
     * result.SetActive(true);
     * waterPark.AddItem(pickupable);
     * WaterParkCreature_BornAsync_Patch.Callback(pickupable); <--- INSERTED LINE
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Finds the waterPark parameter reference so that we can duplicate it
        CodeMatcher waterParkMatcher = new CodeMatcher(instructions).MatchEndForward([
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Ldc_I4_1),
            new CodeMatch(OpCodes.Callvirt),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld),
        ]);

        CodeInstruction ldfldWaterPark = waterParkMatcher.Instruction.Clone();

        CodeMatcher matcher = new CodeMatcher(instructions).MatchEndForward([
            new CodeMatch(OpCodes.Stfld),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld),
            new CodeMatch(OpCodes.Brfalse)
        ]);

        CodeInstruction brFalseInstruction = matcher.Instruction.Clone();

        // 1st injection
        return matcher.Advance(1)
                      .Insert([
                          new CodeInstruction(OpCodes.Ldarg_0),
                          ldfldWaterPark,
                          new CodeInstruction(OpCodes.Call, Reflect.Method(() => CanBeBorn(default))),
                          brFalseInstruction
                       ])
                       // 2nd injection
                       .MatchEndForward([
                          new CodeMatch(OpCodes.Ldarg_0),
                          new CodeMatch(OpCodes.Ldfld),
                          new CodeMatch(OpCodes.Ldloc_3),
                          new CodeMatch(OpCodes.Callvirt)
                      ])
                      .Advance(1)
                      .InsertAndAdvance([
                          new CodeInstruction(OpCodes.Ldloc_3),
                          new CodeInstruction(OpCodes.Call, Reflect.Method(() => Callback(default)))
                      ])
                      .InstructionEnumeration();
    }

    public static bool CanBeBorn(WaterParkCreature waterPark)
    {
        if (waterPark.TryGetNitroxId(out NitroxId waterParkId))
        {
            return Resolve<SimulationOwnership>().HasAnyLockType(waterParkId);
        }

        return true;
    }

    private static void Callback(Pickupable pickupable)
    {
        NitroxEntity.SetNewId(pickupable.gameObject, new());
        Resolve<Items>().Dropped(pickupable.gameObject);
    }
}
