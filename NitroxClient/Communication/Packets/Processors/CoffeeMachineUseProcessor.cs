using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using static Nitrox.Model.Subnautica.Packets.CoffeeMachineUse;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CoffeeMachineUseProcessor : IClientPacketProcessor<CoffeeMachineUse>
{
    private readonly LocalPlayer localPlayer;
    private readonly float machineSoundRange;

    public CoffeeMachineUseProcessor(LocalPlayer localPlayer, FMODWhitelist soundWhitelist)
    {
        this.localPlayer = localPlayer;
        soundWhitelist.TryGetSoundData("event:/sub/base/make_coffee", out SoundData coffeeSoundData);
        machineSoundRange = coffeeSoundData.Radius;
    }

    public Task Process(ClientProcessorContext context, CoffeeMachineUse packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject machineGo))
        {
            Log.Warn("Failed to get CoffeeVendingMachine gameobject while processing CoffeeMachineUse packet");
            return Task.CompletedTask;
        }
        if (!machineGo.TryGetComponent(out CoffeeVendingMachine machine))
        {
            Log.Warn("Failed to get CoffeeVendingMachine component while processing CoffeeMachineUse packet");
            return Task.CompletedTask;
        }
        bool bPlaySound = Vector3.Distance(machineGo.transform.position, localPlayer.Body.transform.position) < machineSoundRange;

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
        return Task.CompletedTask;
    }
}
