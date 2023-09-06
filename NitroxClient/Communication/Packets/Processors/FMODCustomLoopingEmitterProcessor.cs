using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODCustomLoopingEmitterProcessor : ClientPacketProcessor<FMODCustomLoopingEmitterPacket>
{
    public override void Process(FMODCustomLoopingEmitterPacket packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.Id, out FMODEmitterController fmodEmitterController))
        {
            Log.Error($"[FMODCustomLoopingEmitterProcessor] Couldn't find {nameof(FMODEmitterController)} on entity {packet.Id}");
            return;
        }

        fmodEmitterController.PlayCustomLoopingEmitter(packet.AssetPath);
    }
}
