using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;
using static Nitrox.Model.Subnautica.Packets.CoffeeMachineUse;

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
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject machineGO))
        {
            Log.Warn("Failed to get CoffeeVendingMachine gameobject while processing CoffeeMachineUse packet");
            return;
        }
        if (!machineGO.TryGetComponent<CoffeeVendingMachine>(out CoffeeVendingMachine machine))
        {
            Log.Warn("Failed to get CoffeeVendingMachine component while processing CoffeeMachineUse packet");
            return;
        }
        bool bPlaySound = Vector3.Distance(machineGO.transform.position, localPlayer.Body.transform.position) < machineSoundRange;

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
