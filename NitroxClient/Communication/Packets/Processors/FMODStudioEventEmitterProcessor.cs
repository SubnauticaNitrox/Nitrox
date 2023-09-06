using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODStudioEventEmitterProcessor : ClientPacketProcessor<FMODStudioEmitterPacket>
{
    public override void Process(FMODStudioEmitterPacket packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.Id, out FMODEmitterController fmodEmitterController))
        {
            Log.Error($"[FMODStudioEventEmitterProcessor] Couldn't find {nameof(FMODEmitterController)} on entity {packet.Id}");
            return;
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
