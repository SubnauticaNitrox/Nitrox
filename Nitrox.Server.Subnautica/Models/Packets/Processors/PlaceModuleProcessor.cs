using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceModuleProcessor : BuildingProcessor<PlaceModule>
{
    public PlaceModuleProcessor(IPacketSender packetSender, BuildingManager buildingManager, PlayerManager playerManager, EntitySimulation entitySimulation) : base(buildingManager, packetSender, entitySimulation) { }

    public override void Process(PlaceModule packet, Player player)
    {
        if (BuildingManager.AddModule(packet))
        {
            if (packet.ModuleEntity.ParentId == null || !packet.ModuleEntity.IsInside)
            {
                TryClaimBuildPiece(packet.ModuleEntity, player);
            }
            PacketSender.SendPacketToOthersAsync(packet, player.SessionId);
        }
    }
}
