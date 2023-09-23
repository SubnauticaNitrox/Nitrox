using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODEventInstanceProcessor : ClientPacketProcessor<FMODEventInstancePacket>
{
    public override void Process(FMODEventInstancePacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject entity))
        {
            Log.Error($"[FMODEventInstanceProcessor] Couldn't find entity {packet.Id}");
            return;
        }

        if (!entity.TryGetComponent(out FMODEmitterController fmodEmitterController))
        {
            fmodEmitterController = entity.AddComponent<FMODEmitterController>();
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
    }
}
