using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public sealed class CoffeeMachineUseProcessor : ClientPacketProcessor<CoffeeMachineUse>
{
    private readonly LocalPlayer localPlayer;
    private readonly float machineSoundRange;

    public CoffeeMachineUseProcessor(LocalPlayer localPlayer, FMODWhitelist soundWhitelist)
    {
        this.localPlayer = localPlayer;
        soundWhitelist.TryGetSoundData("event:/sub/base/make_coffee", out SoundData coffeeSoundData);
        machineSoundRange = coffeeSoundData.Radius;
    }
    public override void Process(CoffeeMachineUse packet)
    {
        GameObject coffeeMachine = NitroxEntity.RequireObjectFrom(packet.Id);
        CoffeeVendingMachine machine = coffeeMachine.RequireComponent<CoffeeVendingMachine>();
        bool bPlaySound = Vector3.Distance(coffeeMachine.transform.position, localPlayer.Body.transform.position) < machineSoundRange;

        if (packet.Slot == CoffeeMachineSlot.ONE)
        {
            if (bPlaySound)
            {
                machine.waterSoundSlot1.Play();
            }
            machine.vfxController.Play(0);
            machine.timeLastUseSlot1 = Time.time;
        }
        else
        {
            if (bPlaySound)
            {
                machine.waterSoundSlot2.Play();
            }
            machine.vfxController.Play(1);
            machine.timeLastUseSlot2 = Time.time;
        }
    }
}
