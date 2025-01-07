using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts Sea Dragons swat attacks on the simulating player.
/// </summary>
public sealed partial class SeaDragonMeleeAttack_SwatAttack_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaDragonMeleeAttack t) => t.SwatAttack(default, default));

    // All calls sources are either from a simulating player or from a packet processor

    /*
     * MODIFIED:
     *     global::Utils.PlayFMODAsset(this.swatAttackSound, target.transform, 20f);
     * }
     * BroadcastSwatAttack(this, target, isRightHand);          <---- INSERTED LINE
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // 1st injection
        return new CodeMatcher(instructions).End()
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldarg_1),
                                                new CodeInstruction(OpCodes.Ldarg_2),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastSwatAttack(default, default, default)))
                                            ])
                                            .InstructionEnumeration();
    }

    public static void BroadcastSwatAttack(SeaDragonMeleeAttack seaDragonMeleeAttack, GameObject target, bool isRightHand)
    {
        if (seaDragonMeleeAttack.TryGetNitroxId(out NitroxId seaDragonId) &&
            target.TryGetNitroxId(out NitroxId targetId))
        {
            Resolve<IPacketSender>().Send(new SeaDragonSwatAttack(seaDragonId, targetId, isRightHand, seaDragonMeleeAttack.seaDragon.Aggression.Value));
        }
    }
}
