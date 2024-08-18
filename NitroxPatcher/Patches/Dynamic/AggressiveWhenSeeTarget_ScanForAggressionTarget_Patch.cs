using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class AggressiveWhenSeeTarget_ScanForAggressionTarget_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((AggressiveWhenSeeTarget t) => t.ScanForAggressionTarget());

    public static bool Prefix(AggressiveWhenSeeTarget __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId creatureId) ||
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }

        return false;
    }

    /*
     * 
     * Debug.DrawLine(aggressionTarget.transform.position, base.transform.position, Color.white);
     * this.creature.Aggression.Add(num6);
     * BroadcastTargetChange(this, aggressionTarget);   <--- [INSERTED LINE]
     * this.lastTarget.SetTarget(aggressionTarget);
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward(new CodeMatch(OpCodes.Callvirt, Reflect.Method((CreatureTrait t) => t.Add(default))))
                                            .Advance(1)
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldloc_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastTargetChange(default, default)))
                                            ]).InstructionEnumeration();
    }

    public static void BroadcastTargetChange(AggressiveWhenSeeTarget aggressiveWhenSeeTarget, GameObject aggressionTarget)
    {
        if (!Resolve<AI>().IsCreatureWhitelisted(aggressiveWhenSeeTarget.creature))
        {
            return;
        }

        // If the function was called to this point, either it'll return because it doesn't have an id or it'll be evident that we have ownership over the aggressive creature
        LastTarget lastTarget = aggressiveWhenSeeTarget.lastTarget;
        // If there's already (likely another) locked target, we get its id over aggressionTarget
        GameObject realTarget = lastTarget.targetLocked ? lastTarget.target : aggressionTarget;

        if (realTarget && realTarget.TryGetNitroxId(out NitroxId targetId) &&
            aggressiveWhenSeeTarget.TryGetNitroxId(out NitroxId creatureId))
        {
            float aggressionAmount = aggressiveWhenSeeTarget.creature.Aggression.Value;
#if SUBNAUTICA
            Resolve<IPacketSender>().Send(new AggressiveWhenSeeTargetChanged(creatureId, targetId, lastTarget.targetLocked, aggressionAmount));
#elif BELOWZERO
            float targetPriority = aggressiveWhenSeeTarget.targetPriority;

            Resolve<IPacketSender>().Send(new AggressiveWhenSeeTargetChanged(creatureId, targetId, lastTarget.targetLocked, aggressionAmount, targetPriority));
#endif
        }
    }
}
