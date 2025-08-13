using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;
public class CoffeeMachineUseProcessor : ClientPacketProcessor<CoffeeMachineUse>
{
    public override void Process(CoffeeMachineUse packet)
    {
        GameObject coffeeMachine = NitroxEntity.RequireObjectFrom(packet.Id);
        CoffeeVendingMachine machine = coffeeMachine.RequireComponent<CoffeeVendingMachine>();
        if(packet.Slot == CoffeeMachineSlot.ONE)
        {
            machine.vfxController.Play(0);
            machine.waterSoundSlot1.Play();
            machine.timeLastUseSlot1 = Time.time;
        } else
        {
            machine.vfxController.Play(1);
            machine.waterSoundSlot2.Play();
            machine.timeLastUseSlot2 = Time.time;
        }
    }
}
