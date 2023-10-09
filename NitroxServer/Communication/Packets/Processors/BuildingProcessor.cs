using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors;

public abstract class BuildingProcessor<T> : AuthenticatedPacketProcessor<T> where T : Packet
{
    internal readonly BuildingManager buildingManager;
    internal readonly PlayerManager playerManager;

    public BuildingProcessor(BuildingManager buildingManager, PlayerManager playerManager)
    {
        this.buildingManager = buildingManager;
        this.playerManager = playerManager;
    }

    public void SendToOtherPlayersWithOperationId(T packet, Player player, int operationId)
    {
        if (packet is OrderedBuildPacket buildPacket)
        {
            buildPacket.OperationId = operationId;
        }
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
