using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class InfectionReveal_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((Player t) => t.TriggerInfectionRevealAsync()));
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*
From:
            if (!this.infectionRevealed){
    float num = this.armsController.StartHolsterTime(12f);

To:
            if (!this.infectionRevealed){
    float num = this.armsController.StartHolsterTime(12f);
    SendInfectAnimationStartPacket();
 */
        return new CodeMatcher(instructions)
       .MatchStartForward(
           new CodeMatch(OpCodes.Ldloc_1),
           new CodeMatch(OpCodes.Ldfld),
           new CodeMatch(OpCodes.Ldc_R4),
           new CodeMatch(OpCodes.Callvirt, Reflect.Method((ArmsController arms) => arms.StartHolsterTime(default)))
       )
       .Insert(
           new CodeInstruction(OpCodes.Call, Reflect.Method(() => SendInfectAnimationStartPacket()))
       )
       .InstructionEnumeration();
    }
    public static void SendInfectAnimationStartPacket()
    {
        if (!Resolve<LocalPlayer>().PlayerId.HasValue)
        {
            return;
        }
        Log.Debug("Infection animation start packet sending");
        AnimationChangeEvent animEventPacket = new(Resolve<LocalPlayer>().PlayerId.Value, (int)AnimChangeType.INFECTION_REVEAL, (int)AnimChangeState.ON);
        Resolve<IPacketSender>().Send(animEventPacket);
        UWE.CoroutineHost.StartCoroutine(SendInfectAnimationEndPacket());
    }
    public static IEnumerator SendInfectAnimationEndPacket()
    {
        if (!Resolve<LocalPlayer>().PlayerId.HasValue)
        {
            yield break;
        }
        yield return new WaitForSeconds(12f);
        Log.Debug("Infection animation end packet sending");
        AnimationChangeEvent animEventPacket = new(Resolve<LocalPlayer>().PlayerId.Value, (int)AnimChangeType.INFECTION_REVEAL, (int)AnimChangeState.OFF);
        Resolve<IPacketSender>().Send(animEventPacket);
        yield break;
    }
}
