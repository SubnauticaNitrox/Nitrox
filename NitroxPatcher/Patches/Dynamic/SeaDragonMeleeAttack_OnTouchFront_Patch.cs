using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents non simulating players from running locally <see cref="SeaDragonMeleeAttack.OnTouchFront"/>.
/// Broadcasts for the simulating player:
/// - attack on cyclops effects
/// - attack on local player effects
/// - attack on remote players effects
/// </summary>
public sealed partial class SeaDragonMeleeAttack_OnTouchFront_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaDragonMeleeAttack t) => t.OnTouchFront(default));

    public static bool Prefix(SeaDragonMeleeAttack __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId creatureId) ||
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }

        return false;
    }

    /*
     * 1st injection:
     * base.gameObject.SendMessage("OnMeleeAttack", target, SendMessageOptions.DontRequireReceiver);
     * this.timeLastBite = Time.time;
     * BroadcastSeaDragonAttackTarget(this, target);           <---- INSERTED LINE
     * 
     * 2nd injection:
     *     global::Utils.PlayEnvSound(this.attackSound, collider.transform.position, 20f);
     * }
     * component3.gameObject.GetComponent<LiveMixin>().TakeDamage(this.biteDamage, default(Vector3), DamageType.Normal, base.gameObject);
     * BroadcastSeaDragonAttackTarget(this, target);       <---- INSERTED LINE
     * 
     * 3rd injection:
     * Exosuit component4 = target.GetComponent<Exosuit>();
     * BroadcastSeaDragonAttackRemotePlayer(this, target);            <---- INSERTED LINE
     * if (component4 != null)
     * {
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // 1st injection
        return new CodeMatcher(instructions).MatchEndForward([
                                                new CodeMatch(OpCodes.Ldarg_0),
                                                new CodeMatch(OpCodes.Call, Reflect.Property(() => Time.time).GetGetMethod()),
                                                new CodeMatch(OpCodes.Stfld, Reflect.Field((MeleeAttack t) => t.timeLastBite)),
                                                new CodeMatch(OpCodes.Ret)
                                            ])
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldloc_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastSeaDragonAttackTarget(default, default))),
                                            ])
                                            // 2nd injection
                                            .MatchEndForward([
                                                new CodeMatch(OpCodes.Call, Reflect.Property((Component t) => t.gameObject).GetGetMethod()),
                                                new CodeMatch(OpCodes.Callvirt, Reflect.Method((LiveMixin t) => t.TakeDamage(default, default, default, default))),
                                                new CodeMatch(OpCodes.Pop),
                                                new CodeMatch(OpCodes.Br)
                                            ])
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldloc_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastSeaDragonAttackTarget(default, default))),
                                            ])
                                            // 3rd injection
                                            .MatchEndForward([
                                                new CodeMatch(OpCodes.Ldloc_0),
                                                new CodeMatch(OpCodes.Callvirt, Reflect.Method((GameObject t) => t.GetComponent<Exosuit>())),
                                                new CodeMatch(OpCodes.Stloc_S),
                                                new CodeMatch(OpCodes.Ldloc_S),
                                            ])
                                            .InsertAndAdvance([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Ldloc_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => BroadcastSeaDragonAttackRemotePlayer(default, default))),
                                            ])
                                            .InstructionEnumeration();
    }

    public static void BroadcastSeaDragonAttackTarget(SeaDragonMeleeAttack seaDragonMeleeAttack, GameObject target)
    {
        if (seaDragonMeleeAttack.TryGetNitroxId(out NitroxId seaDragonId) &&
            target.TryGetNitroxId(out NitroxId targetId))
        {
            Resolve<IPacketSender>().Send(new SeaDragonAttackTarget(seaDragonId, targetId, seaDragonMeleeAttack.seaDragon.Aggression.Value));
        }
    }

    public static void BroadcastSeaDragonAttackRemotePlayer(SeaDragonMeleeAttack seaDragonMeleeAttack, GameObject target)
    {
        if (target.GetComponent<Exosuit>())
        {
            return;
        }
        if (seaDragonMeleeAttack.TryGetNitroxId(out NitroxId seaDragonId) &&
            target.TryGetComponent(out RemotePlayerIdentifier remotePlayerIdentifier))
        {
            Resolve<IPacketSender>().Send(new SeaDragonAttackTarget(seaDragonId, remotePlayerIdentifier.RemotePlayer.PlayerContext.PlayerNitroxId, seaDragonMeleeAttack.seaDragon.Aggression.Value));
        }
    }
}
