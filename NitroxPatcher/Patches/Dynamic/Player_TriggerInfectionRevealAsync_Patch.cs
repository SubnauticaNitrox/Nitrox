#if SUBNAUTICA
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Player_TriggerInfectionRevealAsync_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((Player t) => t.TriggerInfectionRevealAsync()));
    
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
/*
From:
    if (!this.infectionRevealed){
	    float num = this.armsController.StartHolsterTime(12f);

To:
    if (!this.infectionRevealed){
        SendInfectAnimationStartPacket();
        float num = this.armsController.StartHolsterTime(12f);
 */
        return new CodeMatcher(instructions).MatchStartForward(
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

    private static void SendInfectAnimationStartPacket()
    {
        Log.Debug("Infection animation started");
        Resolve<LocalPlayer>().AnimationChange(AnimChangeType.INFECTION_REVEAL, AnimChangeState.ON);    
    }
}
#endif
