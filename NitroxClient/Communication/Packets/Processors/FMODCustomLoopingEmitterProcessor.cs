using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODCustomLoopingEmitterProcessor : ClientPacketProcessor<FMODCustomLoopingEmitterPacket>
{
    public override void Process(FMODCustomLoopingEmitterPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject entity))
        {
            Log.Error($"[FMODCustomLoopingEmitterProcessor] Couldn't find entity {packet.Id}");
            return;
        }

        if (!entity.TryGetComponent(out FMODEmitterController fmodEmitterController))
        {
            fmodEmitterController = entity.AddComponent<FMODEmitterController>();
            fmodEmitterController.LateRegisterEmitter();
        }

        fmodEmitterController.PlayCustomLoopingEmitter(packet.AssetPath);
    }
}
