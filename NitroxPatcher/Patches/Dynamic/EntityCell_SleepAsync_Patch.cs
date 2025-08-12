using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Entity cells will go sleep when the player gets out of range.  This needs to be reported to the server so they can lose simulation locks.
/// </summary>
public sealed partial class EntityCell_SleepAsync_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((EntityCell t) => t.SleepAsync(default)));

    /*
     * this.state = EntityCell.State.InSleepAsync;
     * EntityCell_SleepAsync_Patch.Callback(this); <--- INSERTED LINE
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Ldloc_1),
                                                new CodeMatch(OpCodes.Ldc_I4_7),
                                                new CodeMatch(OpCodes.Stfld, Reflect.Field((EntityCell t) => t.state))
                                            ])
                                            .Advance(1)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldloc_1),
                                                new CodeInstruction(OpCodes.Call, ((Action<EntityCell>)Callback).Method)
                                            ]).InstructionEnumeration();
    }

    public static void Callback(EntityCell entityCell)
    {
        Resolve<Terrain>().CellUnloaded(entityCell.BatchId, entityCell.CellId, entityCell.Level);
    }
}
