using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class DeathMarkersChangedProcessor : ClientPacketProcessor<DeathMarkersChanged>
{
    private readonly LocalPlayer localPlayer;

    public DeathMarkersChangedProcessor(LocalPlayer localPlayer)
    {
        this.localPlayer = localPlayer;
    }

    public override void Process(DeathMarkersChanged packet)
    {
        localPlayer.MarkDeathPointsWithBeacon = packet.MarkDeathPointsWithBeacon;
    }
}
