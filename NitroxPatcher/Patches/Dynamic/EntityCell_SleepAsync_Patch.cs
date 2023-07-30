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
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((EntityCell t) => t.SleepAsync(default(ProtobufSerializer))));

    public static readonly OpCode INJECTION_OPCODE = OpCodes.Stfld;
    public static readonly object INJECTION_OPERAND = Reflect.Field((EntityCell entityCell) => entityCell.state);

    public static int INJECTION_POSITION = 2;

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        int validSpotsSeen = 0;

        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;

            bool validInjectionInstruction = instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND);

            if (validInjectionInstruction && ++validSpotsSeen == INJECTION_POSITION)
            {
                /*
                 * Injects:  Callback(this);
                 */
                yield return TranspilerHelper.Ldloc<EntityCell>(original);
                yield return new CodeInstruction(OpCodes.Call, ((Action<EntityCell>)Callback).Method);
            }
        }
    }

    public static void Callback(EntityCell entityCell)
    {
        Resolve<Terrain>().CellUnloaded(entityCell.BatchId, entityCell.CellId, entityCell.Level);
    }
}
