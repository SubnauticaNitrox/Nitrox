using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Stops broadcasting position of a pipe surface floater when it has been deployed because it will no longer move.
/// </summary>
public sealed partial class PipeSurfaceFloater_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((PipeSurfaceFloater t) => t.FixedUpdate());

    /*
     *     this.deployed = true;
     *     PipeSurfaceFloater_FixedUpdate_Patch.StopWatching(this); <--- [INSERTED LINE]
     * }
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Insert an instruction right before the Ret
        return new CodeMatcher(instructions).End()
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => StopWatching(default)))
                                            ]).InstructionEnumeration();
    }

    public static void StopWatching(PipeSurfaceFloater pipeSurfaceFloater)
    {
        if (pipeSurfaceFloater.TryGetIdOrWarn(out NitroxId pipeFloaterId) && Resolve<SimulationOwnership>().HasAnyLockType(pipeFloaterId))
        {
            Resolve<SimulationOwnership>().StopSimulatingEntity(pipeFloaterId);
            EntityPositionBroadcaster.StopWatchingEntity(pipeFloaterId);
        }
    }
}
