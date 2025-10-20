using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class LargeWaterParkDeconstructedProcessor : BuildingProcessor<LargeWaterParkDeconstructed>
{
    public LargeWaterParkDeconstructedProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(LargeWaterParkDeconstructed packet, Player player)
    {
        // SeparateChildrenToWaterParks must happen before ReplacePieceByGhost
        // so the water park's children can be moved before it being removed
        if (buildingManager.SeparateChildrenToWaterParks(packet) &&
            buildingManager.ReplacePieceByGhost(player, packet, out _, out int operationId))
        {
            packet.BaseData = null;
            SendToOtherPlayersWithOperationId(packet, player, operationId);
        }
    }
}
