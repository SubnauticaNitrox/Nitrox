using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODCustomEmitterProcessor : ClientPacketProcessor<FMODCustomEmitterPacket>
{
    public override void Process(FMODCustomEmitterPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject entity))
        {
            Log.Error($"[FMODCustomEmitterProcessor] Couldn't find entity {packet.Id}");
            return;
        }

        if (!entity.TryGetComponent(out FMODEmitterController fmodEmitterController))
        {
            fmodEmitterController = entity.AddComponent<FMODEmitterController>();
            fmodEmitterController.LateRegisterEmitter();
        }

        using (PacketSuppressor<FMODCustomEmitterPacket>.Suppress())
        using (PacketSuppressor<FMODCustomLoopingEmitterPacket>.Suppress())
        {
            if (packet.Play)
            {
                fmodEmitterController.PlayCustomEmitter(packet.AssetPath);
            }
            else
            {
                fmodEmitterController.StopCustomEmitter(packet.AssetPath);
            }
        }
    }
}
