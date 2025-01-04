using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Patch responsible for broadcasting <see cref="AttackCyclops"/>' latest data and cancelling <see cref="AttackCyclops.UpdateAggression"/> on players not simulating the creature.
/// </summary>
public sealed partial class AttackCyclops_UpdateAggression_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((AttackCyclops t) => t.UpdateAggression());

    // TODO: Sync attacksub command

    public static bool Prefix(AttackCyclops __instance)
    {
        return !__instance.TryGetNitroxId(out NitroxId creatureId) ||
               Resolve<SimulationOwnership>().HasAnyLockType(creatureId);
    }

    /*
     * REPLACE:
     * if (Player.main != null && Player.main.currentSub != null && Player.main.currentSub.isCyclops)
     * {
     *     cyclopsNoiseManager = Player.main.currentSub.noiseManager;
     * }
     * else if (this.forcedNoiseManager != null)
     * {
     *     cyclopsNoiseManager = this.forcedNoiseManager;
     * }
     * BY:
     * cyclopsNoiseManager = AttackCyclops_UpdateAggression_Patch.FindClosestCyclopsNoiseManagerIfAny(this);
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward([
                                                new CodeMatch(OpCodes.Ldsfld),
                                                new CodeMatch(OpCodes.Ldnull),
                                                new CodeMatch(OpCodes.Call),
                                                new CodeMatch(OpCodes.Brfalse),
                                                new CodeMatch(OpCodes.Ldsfld),
                                                new CodeMatch(OpCodes.Callvirt, Reflect.Property((Player t) => t.currentSub).GetGetMethod())
                                            ]).RemoveInstructions(25)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => FindClosestCyclopsNoiseManagerIfAny(default)))
                                            ]).InstructionEnumeration();
    }

    public static void Postfix(AttackCyclops __instance)
    {
        if (__instance.currentTarget && __instance.currentTarget.TryGetNitroxId(out NitroxId targetId) &&
            __instance.TryGetNitroxId(out NitroxId creatureId) && Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            float aggressiveToNoise = __instance.aggressiveToNoise.Value;

            Resolve<IPacketSender>().Send(new AttackCyclopsTargetChanged(creatureId, targetId, aggressiveToNoise));
        }
    }

    /// <summary>
    /// This is executed every 0.5s for each spawned leviathan so we can focus on optimization
    /// </summary>
    public static CyclopsNoiseManager FindClosestCyclopsNoiseManagerIfAny(AttackCyclops attackCyclops)
    {
        // Limit for optimization
        if (NitroxCyclops.ScaledNoiseByCyclops.Count > 100)
        {
            // Cyclops are marked with EcoTargetType.Whale
            IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Whale, attackCyclops.transform.position, IsTargetAValidInhabitedCyclops, 3);
            if (ecoTarget == null)
            {
                return null;
            }
            
            return ecoTarget.GetGameObject().GetComponent<CyclopsNoiseManager>();
        }
        
        float minDistance = float.MaxValue;
        NitroxCyclops closest = null;
        foreach (KeyValuePair<NitroxCyclops, float> cyclopsEntry in NitroxCyclops.ScaledNoiseByCyclops)
        {
            if (cyclopsEntry.Key.Pawns.Count == 0)
            {
                continue;
            }

            // Calculations from the "if (closestDecoy != null || cyclopsNoiseManager != null)" part
            float distance = Vector3.Distance(cyclopsEntry.Key.transform.position, attackCyclops.transform.position);
            
            if (distance < cyclopsEntry.Value && distance < minDistance)
            {
                minDistance = distance;
                closest = cyclopsEntry.Key;
            }
        }

        return closest.AliveOrNull()?.GetComponent<CyclopsNoiseManager>();
    }

    public static bool IsTargetAValidInhabitedCyclops(IEcoTarget target)
    {
        return IsTargetAValidInhabitedCyclops(target.GetGameObject());
    }

    public static bool IsTargetAValidInhabitedCyclops(GameObject targetObject)
    {
        return targetObject.TryGetComponent(out NitroxCyclops nitroxCyclops) && nitroxCyclops.Pawns.Count > 0;
    }
}
