using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class WaterParkDeconstructedProcessor : BuildingProcessor<WaterParkDeconstructed>
{
    public WaterParkDeconstructedProcessor(BuildingManager buildingManager, IPacketSender packetSender) : base(buildingManager, packetSender) { }

    public override void Process(WaterParkDeconstructed packet, Player player)
    {
        if (BuildingManager.ReplacePieceByGhost(player, packet, out Entity removedEntity, out int operationId) &&
            BuildingManager.CreateWaterParkPiece(packet, removedEntity))
        {
            packet.BaseData = null;
            SendToOtherPlayersWithOperationId(packet, player, operationId);
        }
    }
}
