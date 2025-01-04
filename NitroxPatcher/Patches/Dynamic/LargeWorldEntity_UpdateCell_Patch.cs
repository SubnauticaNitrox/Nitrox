using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class LargeWorldEntity_UpdateCell_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((LargeWorldEntity t) => t.UpdateCell(default));

    /*
     * MODIFIED LINE:
     * streamer.cellManager.RegisterEntity(this);
     * 
     * BECOMES:
     * LargeWorldEntity_UpdateCell_Patch.EntityUpdated(streamer.cellManager.RegisterEntity(this), this);
     * 
     * For this we only need to take the result of RegisterEntity (which is already on the stack) and instead of removing it (pop instruction)
     * we feed it to our method
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Ldarg_1),
                                                new CodeMatch(OpCodes.Ldfld),
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Callvirt),
                                                new CodeMatch(OpCodes.Pop),
                                            ])
                                            .RemoveInstruction() // Remove Pop instruction
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0), // this
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => EntityUpdated(default, default)))
                                            ]).InstructionEnumeration();
    }

    private static void EntityUpdated(bool success, LargeWorldEntity largeWorldEntity)
    {
        // Successfully moving from one cell to another isn't interesting to us
        if (success)
        {
            return;
        }
        
        // The entity has left the regular loaded zone
        // NB: Very important to check for selfActive because it's false when the entity is unloaded by EntityCell.AddEntity

        if (largeWorldEntity.gameObject.activeSelf)
        {
            if (largeWorldEntity.TryGetNitroxId(out NitroxId nitroxId))
            {
                largeWorldEntity.gameObject.EnsureComponent<OutOfCellEntity>().Init(nitroxId);
            }
        }
        // Entity was added to a sleeping cell so it'll be just like it's 
        else if (largeWorldEntity.TryGetComponent(out OutOfCellEntity outOfCellEntity))
        {
            Object.Destroy(outOfCellEntity);
        }
    }
}
