using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CoffeeVendingMachine_OnMachineUse_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((CoffeeVendingMachine t) => t.OnMachineUse(default));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Callvirt, Reflect.Method((VFXController t) => t.Play(default)))
            )
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, Reflect.Method(() => UseCoffeeMachineSlot1(default)))
            )
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Callvirt, Reflect.Method((VFXController t) => t.Play(default)))
            )
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, Reflect.Method(() => UseCoffeeMachineSlot2(default)))
            )
            .InstructionEnumeration();
    }

    private static void UseCoffeeMachineSlot1(CoffeeVendingMachine __instance)
    {
        UseCoffeeMachine(__instance, CoffeeMachineSlot.ONE);
    }

    private static void UseCoffeeMachineSlot2(CoffeeVendingMachine __instance)
    {
        UseCoffeeMachine(__instance, CoffeeMachineSlot.TWO);
    }

    private static void UseCoffeeMachine(CoffeeVendingMachine __instance, CoffeeMachineSlot slot)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId nitroxId))
        {
            Resolve<IPacketSender>().Send(new CoffeeMachineUse(nitroxId, slot));
        }
    }
}
