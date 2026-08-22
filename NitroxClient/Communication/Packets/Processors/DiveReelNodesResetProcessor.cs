using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DiveReelNodesResetProcessor : ClientPacketProcessor<DiveReelNodesReset>
{
    private readonly DiveReelNodeMarkers nodeMarkers;

    public DiveReelNodesResetProcessor(DiveReelNodeMarkers nodeMarkers)
    {
        this.nodeMarkers = nodeMarkers;
    }

    public override void Process(DiveReelNodesReset packet)
    {
        nodeMarkers.ClearMarkers(packet.PlayerId);
    }
}
