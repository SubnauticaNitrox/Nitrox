using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODStudioEventEmitterProcessor : ClientPacketProcessor<FMODStudioEmitterPacket>
{
    public override void Process(FMODStudioEmitterPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject entity))
        {
            Log.Error($"[FMODStudioEventEmitterProcessor] Couldn't find entity {packet.Id}");
            return;
        }

        if (!entity.TryGetComponent(out FMODEmitterController fmodEmitterController))
        {
            fmodEmitterController = entity.AddComponent<FMODEmitterController>();
            fmodEmitterController.LateRegisterEmitter();
        }

        using (PacketSuppressor<FMODStudioEmitterPacket>.Suppress())
        {
            if (packet.Play)
            {
                fmodEmitterController.PlayStudioEmitter(packet.AssetPath);
            }
            else
            {
                fmodEmitterController.StopStudioEmitter(packet.AssetPath, packet.AllowFadeout);
            }
        }
    }
}
