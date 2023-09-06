using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODEventInstanceProcessor : ClientPacketProcessor<FMODEventInstancePacket>
{
    public override void Process(FMODEventInstancePacket packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.Id, out FMODEmitterController fmodEmitterController))
        {
            Log.Error($"[FMODEventInstanceProcessor] Couldn't find {nameof(FMODEmitterController)} on entity {packet.Id}");
            return;
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
