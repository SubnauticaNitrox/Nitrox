using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
/// Cancels <see cref="AttackCyclops.OnCollisionEnter"/> on players not simulating the creature.
/// Replaces the bad cyclops detection to also find out about remote players in the collisioned cyclops.
/// </summary>
internal sealed partial class AttackCyclops_OnCollisionEnter_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    public static MethodInfo TARGET_METHOD = Reflect.Method((AttackCyclops t) => t.OnCollisionEnter(default));

    public AttackCyclops_OnCollisionEnter_Patch(SimulationOwnership so)
    {
        simulationOwnership = so;
    }

    public static bool Prefix(AttackCyclops __instance)
    {
        return !__instance.TryGetNitroxId(out NitroxId creatureId) ||
               simulationOwnership.HasAnyLockType(creatureId);
    }

    /*
     * REPLACE:
     * if (Player.main != null && Player.main.currentSub != null && Player.main.currentSub.isCyclops && Player.main.currentSub.gameObject == collision.gameObject)
     * {
     *     if (this.isActive)
     * BY:
     * if (AttackCyclops_OnCollisionEnter_Patch.ShouldCollisionAnnoyCreature(collision))
     * {
     *     if (this.isActive)
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).Advance(1)
                                            .RemoveInstructions(19)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_1),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => ShouldCollisionAnnoyCreature(default)))
                                            ]).InstructionEnumeration();
    }

    public static bool ShouldCollisionAnnoyCreature(Collision collision)
    {
        return AttackCyclops_UpdateAggression_Patch.IsTargetAValidInhabitedCyclops(collision.gameObject);
    }
}
