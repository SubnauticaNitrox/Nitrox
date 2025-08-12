using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors;

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
