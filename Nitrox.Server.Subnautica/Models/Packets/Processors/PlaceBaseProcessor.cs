using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceBaseProcessor : BuildingProcessor<PlaceBase>
{
    public PlaceBaseProcessor(BuildingManager buildingManager, IPacketSender packetSender, EntitySimulation entitySimulation) : base(buildingManager, packetSender, entitySimulation){ }

    public override void Process(PlaceBase packet, Player player)
    {
        if (BuildingManager.CreateBase(packet))
        {
            TryClaimBuildPiece(packet.BuildEntity, player);
            
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            PacketSender.SendPacketToOthersAsync(packet, player.SessionId);
        }
    }
}
