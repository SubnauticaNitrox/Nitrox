using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class PieceDeconstructedProcessor(IPacketSender packetSender, BuildingManager buildingManager) : BuildingProcessor<PieceDeconstructed>(buildingManager, packetSender)
{
    public override void Process(PieceDeconstructed packet, Player player)
    {
        if (BuildingManager.ReplacePieceByGhost(player, packet, out _, out int operationId))
        {
            packet.BaseData = null;
            SendToOtherPlayersWithOperationId(packet, player, operationId);
        }
    }
}
