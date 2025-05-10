using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODEventInstanceProcessor : IClientPacketProcessor<FMODEventInstancePacket>
{
    public Task Process(IPacketProcessContext context, FMODEventInstancePacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject emitterControllerObject))
        {
            Log.ErrorOnce($"[{nameof(FMODEventInstanceProcessor)}] Couldn't find entity {packet.Id}");
            return Task.CompletedTask;
        }

        if (!emitterControllerObject.TryGetComponent(out FMODEmitterController fmodEmitterController))
        {
            fmodEmitterController = emitterControllerObject.AddComponent<FMODEmitterController>();
            fmodEmitterController.LateRegisterEmitter();
        }

        if (packet.Play)
        {
            fmodEmitterController.PlayEventInstance(packet.AssetPath, packet.Volume);
        }
        else
        {
            fmodEmitterController.StopEventInstance(packet.AssetPath);
        }

        return Task.CompletedTask;
    }
}
