using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Registers the awakening of a cell for <see cref="Terrain"/>, and prevents the cell from loading serialized data of any sort.
/// </summary>
public sealed partial class EntityCell_AwakeAsync_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((EntityCell t) => t.AwakeAsync(default)));

    /*
     * this.state = EntityCell.State.InAwakeAsync;
     * EntityCell_AwakeAsync_Patch.Callback(this); <--- INSERTED LINE
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Ldloc_2),
                                                new CodeMatch(OpCodes.Ldc_I4_6),
                                                new CodeMatch(OpCodes.Stfld, Reflect.Field((EntityCell t) => t.state))
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldloc_2),
                                                new CodeInstruction(OpCodes.Call, ((Action<EntityCell>)Callback).Method)
                                            ]).InstructionEnumeration();
    }

    public static void Callback(EntityCell __instance)
    {
        Resolve<Terrain>().CellLoaded(__instance.BatchId, __instance.CellId, __instance.Level);
        
        __instance.ClearWaiterQueue();
        __instance.serialData.Clear();
        __instance.legacyData.Clear();
        __instance.waiterData.Clear();
    }
}
