using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class PieceDeconstructedProcessor : BuildingProcessor<PieceDeconstructed>
{
    public PieceDeconstructedProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(PieceDeconstructed packet, Player player)
    {
        if (buildingManager.ReplacePieceByGhost(player, packet, out _, out int operationId))
        {
            packet.BaseData = null;
            SendToOtherPlayersWithOperationId(packet, player, operationId);
        }
    }
}
