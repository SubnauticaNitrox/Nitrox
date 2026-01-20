using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class LargeWaterParkDeconstructedProcessor : BuildingProcessor<LargeWaterParkDeconstructed>
{
    public LargeWaterParkDeconstructedProcessor(BuildingManager buildingManager, IPacketSender packetSender) : base(buildingManager, packetSender) { }

    public override void Process(LargeWaterParkDeconstructed packet, Player player)
    {
        // SeparateChildrenToWaterParks must happen before ReplacePieceByGhost
        // so the water park's children can be moved before it being removed
        if (BuildingManager.SeparateChildrenToWaterParks(packet) &&
            BuildingManager.ReplacePieceByGhost(player, packet, out _, out int operationId))
        {
            packet.BaseData = null;
            SendToOtherPlayersWithOperationId(packet, player, operationId);
        }
    }
}
