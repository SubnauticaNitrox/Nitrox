using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using static Nitrox.Model.Packets.CoffeeMachineUse;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CoffeeVendingMachine_OnMachineUse_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((CoffeeVendingMachine t) => t.OnMachineUse(default));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*
         * From:
         * if(Time.time > this.timeLastUseSlot1 + this.useInterval){
         *      this.vfxController.Play(0);
         *  
         * To:
         * if(Time.time > this.timeLastUseSlot1 + this.useInterval){
         *      UseCoffeeMachine(this, CoffeeMachineSlot.ONE);
         *      this.vfxController.Play(0);
         *     
         * From:
         * if(Time.time > this.timeLastUseSlot2 + this.useInterval){
         *      this.vfxController.Play(1);
         * 
         * To:
         * if(Time.time > this.timeLastUseSlot2 + this.useInterval){
         *      UseCoffeeMachine(this, CoffeeMachineSlot.TWO);
         *      this.vfxController.Play(1);
        */
        return new CodeMatcher(instructions)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Callvirt, Reflect.Method((VFXController t) => t.Play(default)))
            )
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4_0), // CoffeeMachineSlot.ONE
                new CodeInstruction(OpCodes.Call, Reflect.Method(() => UseCoffeeMachine(default, default)))
            )
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Callvirt, Reflect.Method((VFXController t) => t.Play(default)))
            )
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4_1), // CoffeeMachineSlot.TWO
                new CodeInstruction(OpCodes.Call, Reflect.Method(() => UseCoffeeMachine(default, default)))
            )
            .InstructionEnumeration();
    }

    private static void UseCoffeeMachine(CoffeeVendingMachine __instance, CoffeeMachineSlot slot)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId nitroxId))
        {
            Resolve<IPacketSender>().Send(new CoffeeMachineUse(nitroxId, slot));
        }
    }
}
