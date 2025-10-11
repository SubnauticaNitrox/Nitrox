using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class WaterParkDeconstructedProcessor : BuildingProcessor<WaterParkDeconstructed>
{
    public WaterParkDeconstructedProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(WaterParkDeconstructed packet, Player player)
    {
        if (buildingManager.ReplacePieceByGhost(player, packet, out Entity removedEntity, out int operationId) &&
            buildingManager.CreateWaterParkPiece(packet, removedEntity))
        {
            packet.BaseData = null;
            SendToOtherPlayersWithOperationId(packet, player, operationId);
        }
    }
}
