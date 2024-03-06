using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class PlaceBaseProcessor : BuildingProcessor<PlaceBase>
{
    public PlaceBaseProcessor(BuildingManager buildingManager, PlayerManager playerManager, EntitySimulation entitySimulation) : base(buildingManager, playerManager, entitySimulation){ }

    public override void Process(PlaceBase packet, Player player)
    {
        if (buildingManager.CreateBase(packet))
        {
            ClaimBuildPiece(packet.BuildEntity, player);
            
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
