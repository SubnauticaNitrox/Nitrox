using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODCustomEmitterProcessor : ClientPacketProcessor<FMODCustomEmitterPacket>
{
    public override void Process(FMODCustomEmitterPacket packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.Id, out FMODEmitterController fmodEmitterController))
        {
            Log.Error($"[FMODCustomEmitterProcessor] Couldn't find {nameof(FMODEmitterController)} on entity {packet.Id}");
            return;
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
