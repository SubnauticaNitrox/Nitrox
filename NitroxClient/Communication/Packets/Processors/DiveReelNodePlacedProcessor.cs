using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DiveReelNodePlacedProcessor : ClientPacketProcessor<DiveReelNodePlaced>
{
    private readonly DiveReelNodeMarkers nodeMarkers;

    public DiveReelNodePlacedProcessor(DiveReelNodeMarkers nodeMarkers)
    {
        this.nodeMarkers = nodeMarkers;
    }

    public override void Process(DiveReelNodePlaced packet)
    {
        nodeMarkers.SpawnMarker(packet.PlayerId, packet.Position);
    }
}
