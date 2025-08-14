using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CoffeeVendingMachine_OnMachineUse_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((CoffeeVendingMachine t) => t.OnMachineUse(default));

    public static void Postfix(CoffeeVendingMachine __instance)
    {
        NitroxId machineId = __instance.RequireComponent<NitroxEntity>().Id;
        CoffeeMachineSlot slot = __instance.waterSoundSlot1.playing ? CoffeeMachineSlot.ONE : CoffeeMachineSlot.TWO;
        Resolve<IPacketSender>().Send(new CoffeeMachineUse(machineId, slot);
    }
}
